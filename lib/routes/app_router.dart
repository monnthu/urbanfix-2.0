import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import '../screens/admin_approval_screen.dart';
import '../screens/civilian_reports_screen.dart';
import '../screens/government_dashboard_screen.dart';
import '../screens/landing_screen.dart';
import '../screens/login_screen.dart';
import '../screens/map_screen.dart';
import '../services/supabase_service.dart';
import '../widgets/app_shell.dart';

class AppRoute {
  const AppRoute._();

  static const landing = '/';
  static const login = '/login';
  static const reports = '/reports';
  static const map = '/map';
  static const dashboard = '/government';
  static const admin = '/admin';
}

final appRouter = GoRouter(
  initialLocation: AppRoute.landing,
  redirect: (context, state) {
    final protectedRoutes = <String>{
      AppRoute.reports,
      AppRoute.dashboard,
      AppRoute.admin,
    };
    final isProtected = protectedRoutes.contains(state.uri.path);
    final isSignedIn = SupabaseService.currentSession != null;

    if (isProtected && !isSignedIn) {
      return '${AppRoute.login}?from=${Uri.encodeComponent(state.uri.path)}';
    }

    return null;
  },
  routes: [
    ShellRoute(
      builder: (context, state, child) {
        return AppShell(currentPath: state.uri.path, child: child);
      },
      routes: [
        GoRoute(
          path: AppRoute.landing,
          pageBuilder: (context, state) => const NoTransitionPage(
            child: LandingScreen(),
          ),
        ),
        GoRoute(
          path: AppRoute.login,
          pageBuilder: (context, state) => NoTransitionPage(
            child: LoginScreen(
              redirectPath: state.uri.queryParameters['from'],
            ),
          ),
        ),
        GoRoute(
          path: AppRoute.reports,
          pageBuilder: (context, state) => const NoTransitionPage(
            child: CivilianReportsScreen(),
          ),
        ),
        GoRoute(
          path: AppRoute.map,
          pageBuilder: (context, state) => const NoTransitionPage(
            child: MapScreen(),
          ),
        ),
        GoRoute(
          path: AppRoute.dashboard,
          pageBuilder: (context, state) => const NoTransitionPage(
            child: GovernmentDashboardScreen(),
          ),
        ),
        GoRoute(
          path: AppRoute.admin,
          pageBuilder: (context, state) => const NoTransitionPage(
            child: AdminApprovalScreen(),
          ),
        ),
      ],
    ),
  ],
  errorBuilder: (context, state) {
    return const Scaffold(
      body: Center(child: Text('Page not found')),
    );
  },
);
