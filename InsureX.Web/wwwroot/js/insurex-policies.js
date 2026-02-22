// insurex-policies.js - Policy Management

$(document).ready(function() {
    initializePolicyManagement();
});

function initializePolicyManagement() {
    // Search form submission
    $('#filterForm').on('submit', function(e) {
        e.preventDefault();
        searchPolicies(1);
    });

    // Handle pagination clicks
    $(document).on('click', '.page-link', function(e) {
        e.preventDefault();
        var page = $(this).data('page');
        if (page) {
            searchPolicies(page);
        }
    });

    // Handle policy actions
    $(document).on('click', '.btn-renew', function(e) {
        e.preventDefault();
        var policyId = $(this).data('policy-id');
        var policyNumber = $(this).data('policy-number');
        showRenewModal(policyId, policyNumber);
    });

    $(document).on('click', '.btn-cancel', function(e) {
        e.preventDefault();
        var policyId = $(this).data('policy-id');
        var policyNumber = $(this).data('policy-number');
        showCancelModal(policyId, policyNumber);
    });

    $(document).on('click', '.btn-payment', function(e) {
        e.preventDefault();
        var policyId = $(this).data('policy-id');
        var premium = $(this).data('premium');
        showPaymentModal(policyId, premium);
    });

    // Modal confirmations
    $('#confirmRenew').on('click', function() {
        renewPolicy();
    });

    $('#confirmCancel').on('click', function() {
        cancelPolicy();
    });

    $('#confirmPayment').on('click', function() {
        processPayment();
    });

    // Initialize any tooltips
    $('[data-bs-toggle="tooltip"]').tooltip();
}

function searchPolicies(page) {
    var searchData = {
        searchTerm: $('input[name="SearchTerm"]').val(),
        status: $('select[name="Status"]').val(),
        policyType: $('select[name="PolicyType"]').val(),
        assetId: $('input[name="AssetId"]').val(),
        expiringOnly: $('#expiringOnly').is(':checked'),
        page: page,
        pageSize: 25,
        sortBy: $('#sortBy').val() || 'enddate',
        sortDir: $('#sortDir').val() || 'asc'
    };

    showLoading();

    $.ajax({
        url: '/Policies/Search',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(searchData),
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(result) {
            $('#policyTableContainer').html(result);
            hideLoading();
            
            // Update URL with search params (optional)
            updateUrlWithSearchParams(searchData);
        },
        error: function(xhr, status, error) {
            console.error('Search failed:', error);
            showNotification('Search failed. Please try again.', 'error');
            hideLoading();
        }
    });
}

function showRenewModal(policyId, policyNumber) {
    $('#renewPolicyId').val(policyId);
    $('#renewModal .modal-title').text('Renew Policy: ' + policyNumber);
    $('#renewModal').modal('show');
}

function renewPolicy() {
    var policyId = $('#renewPolicyId').val();
    var newExpiryDate = $('#newExpiryDate').val();

    if (!newExpiryDate) {
        showNotification('Please select a new expiry date', 'warning');
        return;
    }

    showLoading();

    $.ajax({
        url: '/Policies/Renew/' + policyId,
        type: 'POST',
        data: {
            newExpiryDate: newExpiryDate,
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(response) {
            $('#renewModal').modal('hide');
            if (response.success) {
                showNotification(response.message, 'success');
                searchPolicies(1); // Refresh the list
            } else {
                showNotification(response.message, 'error');
            }
            hideLoading();
        },
        error: function() {
            hideLoading();
            showNotification('Error renewing policy', 'error');
        }
    });
}

function showCancelModal(policyId, policyNumber) {
    $('#cancelPolicyId').val(policyId);
    $('#cancelModal .modal-title').text('Cancel Policy: ' + policyNumber);
    $('#cancelModal').modal('show');
}

function cancelPolicy() {
    var policyId = $('#cancelPolicyId').val();
    var reason = $('#cancelReason').val();

    if (!reason) {
        showNotification('Please provide a cancellation reason', 'warning');
        return;
    }

    showLoading();

    $.ajax({
        url: '/Policies/Cancel/' + policyId,
        type: 'POST',
        data: {
            reason: reason,
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(response) {
            $('#cancelModal').modal('hide');
            if (response.success) {
                showNotification(response.message, 'success');
                searchPolicies(1); // Refresh the list
            } else {
                showNotification(response.message, 'error');
            }
            hideLoading();
        },
        error: function() {
            hideLoading();
            showNotification('Error cancelling policy', 'error');
        }
    });
}

function showPaymentModal(policyId, premium) {
    $('#paymentPolicyId').val(policyId);
    $('#paymentAmount').val(premium);
    $('#paymentModal').modal('show');
}

function processPayment() {
    var policyId = $('#paymentPolicyId').val();
    var payment = {
        status: 'Paid',
        paymentDate: $('#paymentDate').val(),
        reference: $('#paymentReference').val(),
        amount: $('#paymentAmount').val(),
        paymentMethod: $('#paymentMethod').val()
    };

    if (!payment.reference) {
        showNotification('Please enter payment reference', 'warning');
        return;
    }

    showLoading();

    $.ajax({
        url: '/Policies/ProcessPayment/' + policyId,
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(payment),
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(response) {
            $('#paymentModal').modal('hide');
            if (response.success) {
                showNotification(response.message, 'success');
                searchPolicies(1); // Refresh the list
            } else {
                showNotification(response.message, 'error');
            }
            hideLoading();
        },
        error: function() {
            hideLoading();
            showNotification('Error processing payment', 'error');
        }
    });
}

function showLoading() {
    $('#loadingSpinner').show();
}

function hideLoading() {
    $('#loadingSpinner').hide();
}

function showNotification(message, type) {
    // You can use toastr or any other notification library
    if (type === 'success') {
        toastr.success(message);
    } else if (type === 'error') {
        toastr.error(message);
    } else if (type === 'warning') {
        toastr.warning(message);
    } else {
        toastr.info(message);
    }
}

function updateUrlWithSearchParams(params) {
    var url = new URL(window.location);
    Object.keys(params).forEach(key => {
        if (params[key]) {
            url.searchParams.set(key, params[key]);
        } else {
            url.searchParams.delete(key);
        }
    });
    window.history.pushState({}, '', url);
}