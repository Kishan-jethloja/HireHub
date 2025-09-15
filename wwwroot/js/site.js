// Vertical Navigation JavaScript
document.addEventListener('DOMContentLoaded', function() {
    const verticalNav = document.getElementById('verticalNav');
    const mainContent = document.getElementById('mainContent');
    const navToggle = document.getElementById('navToggle');
    const sidebarToggle = document.getElementById('sidebarToggle');
    
    // Toggle sidebar collapse/expand
    if (navToggle) {
        navToggle.addEventListener('click', function() {
            verticalNav.classList.toggle('collapsed');
        });
    }
    
    // Mobile sidebar toggle
    if (sidebarToggle) {
        sidebarToggle.addEventListener('click', function() {
            verticalNav.classList.toggle('mobile-open');
        });
    }
    
    // Close mobile sidebar when clicking outside
    document.addEventListener('click', function(e) {
        if (window.innerWidth <= 768) {
            if (!verticalNav.contains(e.target) && !sidebarToggle.contains(e.target)) {
                verticalNav.classList.remove('mobile-open');
            }
        }
    });
    
    // Handle window resize
    window.addEventListener('resize', function() {
        if (window.innerWidth > 768) {
            verticalNav.classList.remove('mobile-open');
        }
    });
    
    // Active navigation link highlighting
    const currentPath = window.location.pathname;
    const navLinks = document.querySelectorAll('.nav-link');
    
    navLinks.forEach(link => {
        if (link.getAttribute('href') === currentPath) {
            link.classList.add('active');
        }
    });
    
    // Smooth scrolling for anchor links
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });
    
    // Add loading states to buttons
    document.querySelectorAll('button[type="submit"]').forEach(button => {
        button.addEventListener('click', function() {
            if (this.form && this.form.checkValidity()) {
                this.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Processing...';
                this.disabled = true;
            }
        });
    });
    
    // Notification badge animation
    const notificationBadges = document.querySelectorAll('.nav-badge, .badge');
    notificationBadges.forEach(badge => {
        if (badge.textContent !== '0') {
            badge.style.animation = 'pulse 2s infinite';
        }
    });
    
    // User profile dropdown (if needed)
    const userProfile = document.querySelector('.user-profile');
    if (userProfile) {
        userProfile.addEventListener('click', function() {
            // Add dropdown functionality here if needed
            console.log('User profile clicked');
        });
    }
    
    // Search functionality (placeholder)
    const searchButtons = document.querySelectorAll('button[title*="search"], .fa-search');
    searchButtons.forEach(button => {
        button.addEventListener('click', function() {
            // Add search functionality here
            console.log('Search clicked');
        });
    });
    
    // Notification functionality (placeholder)
    const notificationButtons = document.querySelectorAll('button[title*="notification"], .fa-bell');
    notificationButtons.forEach(button => {
        button.addEventListener('click', function() {
            // Add notification functionality here
            console.log('Notifications clicked');
        });
    });
});

// Utility functions
function showNotification(message, type = 'info') {
    // Create notification element
    const notification = document.createElement('div');
    notification.className = `alert alert-${type} alert-dismissible fade show position-fixed`;
    notification.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
    notification.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    
    document.body.appendChild(notification);
    
    // Auto remove after 5 seconds
    setTimeout(() => {
        if (notification.parentNode) {
            notification.remove();
        }
    }, 5000);
}

// Form validation helpers
function validateForm(form) {
    const inputs = form.querySelectorAll('input[required], select[required], textarea[required]');
    let isValid = true;
    
    inputs.forEach(input => {
        if (!input.value.trim()) {
            input.classList.add('is-invalid');
            isValid = false;
        } else {
            input.classList.remove('is-invalid');
            input.classList.add('is-valid');
        }
    });
    
    return isValid;
}

// Loading state helpers
function showLoading(element) {
    const originalContent = element.innerHTML;
    element.setAttribute('data-original-content', originalContent);
    element.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Loading...';
    element.disabled = true;
}

function hideLoading(element) {
    const originalContent = element.getAttribute('data-original-content');
    if (originalContent) {
        element.innerHTML = originalContent;
        element.removeAttribute('data-original-content');
    }
    element.disabled = false;
}
