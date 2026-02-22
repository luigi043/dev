var policyManagement = (function() {
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
            searchPolicies();
        });

        // Pagination clicks
        $(document).on('click', '.page-link-prev, .page-link-next, .page-link-num', function(e) {
            e.preventDefault();
            var page = $(this).data('page');
            if (page) {
                currentPage = page;
                searchPolicies();
            }
        });

        // Sort links
        $(document).on('click', '.sort-link', function(e) {
            e.preventDefault();
            var sortBy = $(this).data('sort');
            toggleSort(sortBy);
            searchPolicies();
        });

        // Delete button
        $(document).on('click', '.delete-btn', function() {
            var id = $(this).data('id');
            var number = $(this).data('number');
            showDeleteModal(id, number);
        });

        // Confirm delete
        $('#confirmDelete').on('click', function() {
            var id = $(this).data('id');
            deletePolicy(id);
        });

        // Filter changes
        $('#statusFilter, #policyTypeFilter, #paymentStatusFilter, #expiringOnly').on('change', function() {
            currentPage = 1;
            searchPolicies();
        });
    }

    function searchPolicies() {
        var searchData = {
            page: currentPage,
            pageSize: 10,
            searchTerm: $('#searchTerm').val(),
            status: $('#statusFilter').val(),
            policyType: $('#policyTypeFilter').val(),
            paymentStatus: $('#paymentStatusFilter').val(),
            expiringOnly: $('#expiringOnly').is(':checked'),
            sortBy: currentSearch.sortBy || 'enddate',
            sortDir: currentSearch.sortDir || 'asc'
        };

        $.ajax({
            url: '/Policies/Search',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(searchData),
            success: function(result) {
                $('#policyTableContainer').html(result);
                currentSearch = searchData;
            },
            error: function() {
                toastr.error('Error searching policies');
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

    function showDeleteModal(id, number) {
        $('#confirmDelete').data('id', id);
        $('#deleteModal .modal-body p:first').text(`Are you sure you want to delete policy "${number}"?`);
        $('#deleteModal').modal('show');
    }

    function deletePolicy(id) {
        $.ajax({
            url: '/Policies/Delete',
            type: 'POST',
            data: { id: id },
            success: function(response) {
                $('#deleteModal').modal('hide');
                if (response.success) {
                    toastr.success(response.message);
                    searchPolicies(); // Refresh the list
                } else {
                    toastr.error(response.message);
                }
            },
            error: function() {
                $('#deleteModal').modal('hide');
                toastr.error('Error deleting policy');
            }
        });
    }

    return {
        init: init
    };
})();