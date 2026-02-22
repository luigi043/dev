var assetManagement = (function() {
    var currentPage = 1;
    var currentSearch = {};

    function init() {
        bindEvents();
    }

    function bindEvents() {
        // Search form submit
        $('#searchForm').on('submit', function(e) {
            e.preventDefault();
            currentPage = 1;
            searchAssets();
        });

        // Pagination clicks
        $(document).on('click', '.page-link-prev, .page-link-next, .page-link-num', function(e) {
            e.preventDefault();
            var page = $(this).data('page');
            if (page) {
                currentPage = page;
                searchAssets();
            }
        });

        // Sort links
        $(document).on('click', '.sort-link', function(e) {
            e.preventDefault();
            var sortBy = $(this).data('sort');
            toggleSort(sortBy);
            searchAssets();
        });

        // Delete button
        $(document).on('click', '.delete-btn', function() {
            var id = $(this).data('id');
            var tag = $(this).data('tag');
            showDeleteModal(id, tag);
        });

        // Confirm delete
        $('#confirmDelete').on('click', function() {
            var id = $(this).data('id');
            deleteAsset(id);
        });

        // Export button
        $('#exportBtn').on('click', function() {
            exportAssets();
        });

        // Auto-submit on filter change
        $('#statusFilter, #complianceFilter, #yearFilter').on('change', function() {
            currentPage = 1;
            searchAssets();
        });
    }

    function searchAssets() {
        var searchData = {
            page: currentPage,
            pageSize: 10,
            searchTerm: $('#searchTerm').val(),
            status: $('#statusFilter').val(),
            complianceStatus: $('#complianceFilter').val(),
            year: $('#yearFilter').val() ? parseInt($('#yearFilter').val()) : null,
            sortBy: currentSearch.sortBy || 'CreatedAt',
            sortDir: currentSearch.sortDir || 'desc'
        };

        $.ajax({
            url: '/Assets/Search',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(searchData),
            success: function(result) {
                $('#assetTableContainer').html(result);
                currentSearch = searchData;
                updateUrl();
            },
            error: function() {
                toastr.error('Error searching assets');
            }
        });
    }

    function toggleSort(sortBy) {
        if (currentSearch.sortBy === sortBy) {
            currentSearch.sortDir = currentSearch.sortDir === 'asc' ? 'desc' : 'asc';
        } else {
            currentSearch.sortBy = sortBy;
            currentSearch.sortDir = 'asc';
        }
    }

    function showDeleteModal(id, tag) {
        $('#confirmDelete').data('id', id);
        $('#deleteModal .modal-body p:first').text(`Are you sure you want to delete asset "${tag}"?`);
        $('#deleteModal').modal('show');
    }

    function deleteAsset(id) {
        $.ajax({
            url: '/Assets/Delete',
            type: 'POST',
            data: { id: id },
            success: function(response) {
                $('#deleteModal').modal('hide');
                if (response.success) {
                    toastr.success(response.message);
                    searchAssets(); // Refresh the list
                } else {
                    toastr.error(response.message);
                }
            },
            error: function() {
                $('#deleteModal').modal('hide');
                toastr.error('Error deleting asset');
            }
        });
    }

    function exportAssets() {
        var searchData = {
            searchTerm: $('#searchTerm').val(),
            status: $('#statusFilter').val(),
            complianceStatus: $('#complianceFilter').val(),
            year: $('#yearFilter').val() ? parseInt($('#yearFilter').val()) : null
        };

        var queryString = $.param(searchData);
        window.location.href = '/Assets/Export?' + queryString;
    }

    function updateUrl() {
        var queryString = $.param({
            page: currentPage,
            searchTerm: currentSearch.searchTerm,
            status: currentSearch.status,
            complianceStatus: currentSearch.complianceStatus,
            year: currentSearch.year
        });
        history.pushState(null, '', '/Assets?' + queryString);
    }

    return {
        init: init
    };
})();