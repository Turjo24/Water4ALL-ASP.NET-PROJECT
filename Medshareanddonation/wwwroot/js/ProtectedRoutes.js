// wwwroot/js/ProtectedRoutes.js
// এই file টি আপনার wwwroot/js folder এ রাখুন

class ProtectedRoutes {
    constructor() {
        this.checkRoute();
    }

    // Check if user is authenticated
    isAuth() {
        const token = localStorage.getItem('token');
        return token !== null && token !== '';
    }

    // Get current page path
    getCurrentPath() {
        return window.location.pathname.toLowerCase  

    // Check if current page is protected
    isProtectedPage() {
        const path = this.getCurrentPath();
        const protectedPaths = ['/admin', '/Home', '/user', '/dashboard', '/profile','auth/SignIn','auth/SignUp'];
        return protectedPaths.some(p => path.startsWith(p));
    }

    // Main route check logic
    checkRoute() {
        const isAuth = this.isAuth();
        const isAuthPage = this.isAuthPage();
        const isProtectedPage = this.isProtectedPage();

        // If logged in and trying to access login/register -> redirect to dashboard
        if (isAuth && isAuthPage) {
            this.redirectToDashboard();
        }

        // If not logged in and trying to access protected page -> redirect to login
        if (!isAuth && isProtectedPage) {
            window.location.replace('auth/SignIn');
        }
    }

    // Redirect to dashboard based on role
    redirectToDashboard() {
        const role = localStorage.getItem('userRole');
        switch (role) {
            case 'Admin':
                window.location.replace('/Home');
                break;
            case 'Volunteer':
                window.location.replace('/Home');
                break;
            default:
                window.location.replace('/Home');
        }
    }
}

// Initialize
new ProtectedRoutes();