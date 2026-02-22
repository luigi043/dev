/**
 * InsureX Asset Management Module
 * Handles all asset-related UI interactions with enhanced security and error handling
 * @version 2.0.0
 */

const assetManagement = (function() {
    'use strict';

    // ==================== Configuration ====================
    const CONFIG = {
        pageSize: 10,
        defaultSortBy: 'CreatedAt',
        defaultSortDir: 'desc',
        debounceDelay: 300,
        maxRetries: 3,
        requestTimeout: 30000 // 30 seconds
    };

    // ==================== State Management ====================
    const state = {
        currentPage: 1,
        currentSearch: {
            searchTerm: '',
            status: '',
            complianceStatus: '',
            year: null,
            sortBy: CONFIG.defaultSortBy,
            sortDir: CONFIG.defaultSortDir
        },
        isLoading: false,
        abortController: null,
        retryCount: 0
    };

    // ==================== Cache DOM elements ====================
    const elements = {
        searchForm: document.getElementById('searchForm'),
        searchTerm: document.getElementById('searchTerm'),
        statusFilter: document.getElementById('statusFilter'),
        complianceFilter: document.getElementById('complianceFilter'),
        yearFilter: document.getElementById('yearFilter'),
        assetTableContainer: document.getElementById('assetTableContainer'),
        exportBtn: document.getElementById('exportBtn'),
        deleteModal: document.getElementById('deleteModal'),
        confirmDeleteBtn: document.getElementById('confirmDelete')
    };

    // ==================== Utility Functions ====================
    const utils = {
        /**
         * Shows a toast notification
         * @param {string} message - Message to display
         * @param {string} type - 'success', 'error', 'warning', 'info'
         */
        showToast: (message, type = 'info') => {
            if (window.toastr) {
                toastr[type](message);
            } else {
                alert(message); // Fallback
            }
        },

        /**
         * Shows a loading indicator
         */
        showLoading: () => {
            state.isLoading = true;
            if (elements.assetTableContainer) {
                elements.assetTableContainer.innerHTML = `
                    <div class="text-center py-5">
                        <div class="spinner-border text-primary" role="status">
                            <span class="visually-hidden">Loading...</span>
                        </div>
                        <p class="mt-2 text-muted">Loading assets...</p>
                    </div>
                `;
            }
        },

        /**
         * Gets CSRF token from meta tag or form
         * @returns {string} CSRF token
         */
        getCsrfToken: () => {
            const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
            return tokenElement ? tokenElement.value : '';
        },

        /**
         * Creates debounced function
         * @param {Function} func - Function to debounce
         * @param {number} delay - Delay in milliseconds
         * @returns {Function} Debounced function
         */
        debounce: (func, delay) => {
            let timeoutId;
            return function(...args) {
                clearTimeout(timeoutId);
                timeoutId = setTimeout(() => func.apply(this, args), delay);
            };
        },

        /**
         * Builds query string from object
         * @param {Object} params - Parameters object
         * @returns {string} Query string
         */
        buildQueryString: (params) => {
            return Object.entries(params)
                .filter(([_, value]) => value !== null && value !== undefined && value !== '')
                .map(([key, value]) => `${encodeURIComponent(key)}=${encodeURIComponent(value)}`)
                .join('&');
        }
    };

    // ==================== API Calls ====================
    const api = {
        /**
         * Makes an AJAX request with retry logic
         * @param {Object} options - Request options
         * @returns {Promise} Promise with response
         */
        async request(options) {
            const defaultOptions = {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': utils.getCsrfToken()
                },
                credentials: 'same-origin',
                timeout: CONFIG.requestTimeout
            };

            // Cancel previous request if exists
            if (state.abortController) {
                state.abortController.abort();
            }

            state.abortController = new AbortController();
            const fetchOptions = {
                ...defaultOptions,
                ...options,
                signal: state.abortController.signal,
                headers: {
                    ...defaultOptions.headers,
                    ...options.headers
                }
            };

            if (options.body) {
                fetchOptions.body = JSON.stringify(options.body);
            }

            try {
                const response = await fetch(options.url, fetchOptions);
                
                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }

                const contentType = response.headers.get('content-type');
                if (contentType && contentType.includes('application/json')) {
                    return await response.json();
                }
                
                return await response.text();
            } catch (error) {
                if (error.name === 'AbortError') {
                    console.log('Request cancelled');
                    return null;
                }
                throw error;
            }
        },

        /**
         * Searches assets
         * @param {Object} searchData - Search parameters
         * @returns {Promise} Promise with search results
         */
        async searchAssets(searchData) {
            return await this.request({
                url: '/Assets/Search',
                method: 'POST',
                body: searchData
            });
        },

        /**
         * Deletes an asset
         * @param {string} id - Asset ID
         * @returns {Promise} Promise with delete response
         */
        async deleteAsset(id) {
            return await this.request({
                url: '/Assets/Delete',
                method: 'POST',
                body: { id }
            });
        },

        /**
         * Exports assets
         * @param {Object} searchData - Search parameters
         */
        exportAssets(searchData) {
            const queryString = utils.buildQueryString(searchData);
            window.location.href = `/Assets/Export?${queryString}`;
        }
    };

    // ==================== Core Functions ====================
    const core = {
        /**
         * Performs asset search
         */
        async searchAssets() {
            if (state.isLoading) return;

            const searchData = {
                page: state.currentPage,
                pageSize: CONFIG.pageSize,
                searchTerm: elements.searchTerm?.value || '',
                status: elements.statusFilter?.value || '',
                complianceStatus: elements.complianceFilter?.value || '',
                year: elements.yearFilter?.value ? parseInt(elements.yearFilter.value) : null,
                sortBy: state.currentSearch.sortBy,
                sortDir: state.currentSearch.sortDir
            };

            try {
                utils.showLoading();
                
                const result = await api.searchAssets(searchData);
                
                if (result) {
                    if (elements.assetTableContainer) {
                        elements.assetTableContainer.innerHTML = result;
                    }
                    
                    // Update state
                    state.currentSearch = { ...searchData };
                    state.retryCount = 0;
                    
                    // Update URL for bookmarking
                    history.pushState(
                        { page: state.currentPage, ...searchData },
                        '',
                        `/Assets?${utils.buildQueryString(searchData)}`
                    );
                }
            } catch (error) {
                console.error('Search failed:', error);
                
                // Retry logic
                if (state.retryCount < CONFIG.maxRetries) {
                    state.retryCount++;
                    utils.showToast(`Retrying... (${state.retryCount}/${CONFIG.maxRetries})`, 'warning');
                    setTimeout(() => core.searchAssets(), 1000 * state.retryCount);
                } else {
                    utils.showToast('Failed to load assets. Please refresh the page.', 'error');
                    state.retryCount = 0;
                }
            } finally {
                state.isLoading = false;
            }
        },

        /**
         * Toggles sort direction
         * @param {string} sortBy - Field to sort by
         */
        toggleSort(sortBy) {
            if (state.currentSearch.sortBy === sortBy) {
                state.currentSearch.sortDir = state.currentSearch.sortDir === 'asc' ? 'desc' : 'asc';
            } else {
                state.currentSearch.sortBy = sortBy;
                state.currentSearch.sortDir = 'asc';
            }
            core.searchAssets();
        },

        /**
         * Shows delete confirmation modal
         * @param {string} id - Asset ID
         * @param {string} tag - Asset tag
         */
        showDeleteModal(id, tag) {
            if (!elements.deleteModal || !elements.confirmDeleteBtn) return;

            // Update modal content
            const messageElement = elements.deleteModal.querySelector('.modal-body p:first-child');
            if (messageElement) {
                messageElement.textContent = `Are you sure you want to delete asset "${tag}"?`;
                messageElement.setAttribute('data-asset-id', id);
                messageElement.setAttribute('data-asset-tag', tag);
            }

            // Store ID in confirm button
            elements.confirmDeleteBtn.setAttribute('data-id', id);
            
            // Show modal
            const modal = new bootstrap.Modal(elements.deleteModal);
            modal.show();
        },

        /**
         * Deletes an asset
         * @param {string} id - Asset ID
         */
        async deleteAsset(id) {
            try {
                const response = await api.deleteAsset(id);
                
                // Hide modal
                if (elements.deleteModal) {
                    const modal = bootstrap.Modal.getInstance(elements.deleteModal);
                    if (modal) modal.hide();
                }

                if (response && response.success) {
                    utils.showToast(response.message || 'Asset deleted successfully', 'success');
                    core.searchAssets(); // Refresh the list
                } else {
                    utils.showToast(response?.message || 'Error deleting asset', 'error');
                }
            } catch (error) {
                console.error('Delete failed:', error);
                utils.showToast('Error deleting asset. Please try again.', 'error');
            }
        },

        /**
         * Exports assets
         */
        exportAssets() {
            const searchData = {
                searchTerm: elements.searchTerm?.value || '',
                status: elements.statusFilter?.value || '',
                complianceStatus: elements.complianceFilter?.value || '',
                year: elements.yearFilter?.value ? parseInt(elements.yearFilter.value) : null
            };

            api.exportAssets(searchData);
        }
    };

    // ==================== Event Handlers ====================
    const handlers = {
        /**
         * Handles search form submission
         * @param {Event} e - Event object
         */
        onSearchSubmit: (e) => {
            e.preventDefault();
            state.currentPage = 1;
            core.searchAssets();
        },

        /**
         * Handles pagination click
         * @param {Event} e - Event object
         */
        onPageClick: (e) => {
            e.preventDefault();
            const page = e.currentTarget.dataset.page;
            if (page) {
                state.currentPage = parseInt(page);
                core.searchAssets();
            }
        },

        /**
         * Handles sort link click
         * @param {Event} e - Event object
         */
        onSortClick: (e) => {
            e.preventDefault();
            const sortBy = e.currentTarget.dataset.sort;
            if (sortBy) {
                core.toggleSort(sortBy);
            }
        },

        /**
         * Handles delete button click
         * @param {Event} e - Event object
         */
        onDeleteClick: (e) => {
            const button = e.currentTarget;
            const id = button.dataset.id;
            const tag = button.dataset.tag;
            if (id && tag) {
                core.showDeleteModal(id, tag);
            }
        },

        /**
         * Handles filter change
         */
        onFilterChange: () => {
            state.currentPage = 1;
            core.searchAssets();
        },

        /**
         * Handles confirm delete
         */
        onConfirmDelete: () => {
            if (elements.confirmDeleteBtn) {
                const id = elements.confirmDeleteBtn.dataset.id;
                if (id) {
                    core.deleteAsset(id);
                }
            }
        },

        /**
         * Handles popstate event (browser back/forward)
         * @param {PopStateEvent} e - Event object
         */
        onPopState: (e) => {
            if (e.state) {
                state.currentPage = e.state.page || 1;
                // Restore filters from URL if needed
                core.searchAssets();
            }
        }
    };

    // ==================== Event Binding ====================
    function bindEvents() {
        // Search form
        if (elements.searchForm) {
            elements.searchForm.addEventListener('submit', handlers.onSearchSubmit);
        }

        // Search term with debounce
        if (elements.searchTerm) {
            elements.searchTerm.addEventListener('input', utils.debounce(() => {
                state.currentPage = 1;
                core.searchAssets();
            }, CONFIG.debounceDelay));
        }

        // Filter changes
        [elements.statusFilter, elements.complianceFilter, elements.yearFilter].forEach(filter => {
            if (filter) {
                filter.addEventListener('change', handlers.onFilterChange);
            }
        });

        // Export button
        if (elements.exportBtn) {
            elements.exportBtn.addEventListener('click', core.exportAssets);
        }

        // Confirm delete
        if (elements.confirmDeleteBtn) {
            elements.confirmDeleteBtn.addEventListener('click', handlers.onConfirmDelete);
        }

        // Dynamic event delegation for table elements
        if (elements.assetTableContainer) {
            elements.assetTableContainer.addEventListener('click', (e) => {
                // Pagination
                if (e.target.closest('.page-link')) {
                    const pageLink = e.target.closest('.page-link');
                    if (pageLink.dataset.page) {
                        e.preventDefault();
                        state.currentPage = parseInt(pageLink.dataset.page);
                        core.searchAssets();
                    }
                }

                // Sort links
                if (e.target.closest('.sort-link')) {
                    const sortLink = e.target.closest('.sort-link');
                    e.preventDefault();
                    core.toggleSort(sortLink.dataset.sort);
                }

                // Delete buttons
                if (e.target.closest('.delete-btn')) {
                    const deleteBtn = e.target.closest('.delete-btn');
                    handlers.onDeleteClick({ currentTarget: deleteBtn });
                }
            });
        }

        // Browser navigation
        window.addEventListener('popstate', handlers.onPopState);
    }

    // ==================== Public API ====================
    return {
        /**
         * Initializes the asset management module
         */
        init: () => {
            try {
                // Load initial state from URL if needed
                const urlParams = new URLSearchParams(window.location.search);
                state.currentPage = parseInt(urlParams.get('page')) || 1;
                state.currentSearch.searchTerm = urlParams.get('searchTerm') || '';
                
                // Set form values from URL
                if (elements.searchTerm) elements.searchTerm.value = state.currentSearch.searchTerm;
                
                // Bind events
                bindEvents();
                
                // Initial search
                core.searchAssets();
                
                console.log('Asset Management module initialized successfully');
            } catch (error) {
                console.error('Failed to initialize Asset Management module:', error);
                utils.showToast('Failed to initialize. Please refresh the page.', 'error');
            }
        },

        /**
         * Refreshes the asset list
         */
        refresh: () => {
            core.searchAssets();
        },

        /**
         * Gets current state
         * @returns {Object} Current state
         */
        getState: () => ({ ...state }),

        /**
         * Sets page size
         * @param {number} size - Page size
         */
        setPageSize: (size) => {
            if (size > 0) {
                CONFIG.pageSize = size;
                core.searchAssets();
            }
        }
    };
})();

// Initialize when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => assetManagement.init());
} else {
    assetManagement.init();
}

// Export for module systems (if needed)
if (typeof module !== 'undefined' && module.exports) {
    module.exports = assetManagement;
}