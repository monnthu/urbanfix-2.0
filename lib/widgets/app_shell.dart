import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import '../routes/app_router.dart';

class AppShell extends StatelessWidget {
  const AppShell({
    required this.currentPath,
    required this.child,
    super.key,
  });

  final String currentPath;
  final Widget child;

  @override
  Widget build(BuildContext context) {
    const destinations = [
      _NavDestination('Home', Icons.home_outlined, AppRoute.landing),
      _NavDestination(
        'Report',
        Icons.add_location_alt_outlined,
        AppRoute.reports,
      ),
      _NavDestination('Map', Icons.map_outlined, AppRoute.map),
      _NavDestination('Gov', Icons.account_balance_outlined, AppRoute.dashboard),
    ];
    final selectedIndex = destinations.indexWhere(
      (destination) => destination.path == currentPath,
    );

    return Scaffold(
      appBar: AppBar(
        title: const Text('urbanfix'),
        actions: [
          TextButton(
            onPressed: () => context.go(AppRoute.login),
            child: const Text('Login'),
          ),
          const SizedBox(width: 8),
        ],
      ),
      body: SafeArea(child: child),
      bottomNavigationBar: NavigationBar(
        selectedIndex: selectedIndex < 0 ? 0 : selectedIndex,
        onDestinationSelected: (index) => context.go(destinations[index].path),
        destinations: [
          for (final destination in destinations)
            NavigationDestination(
              icon: Icon(destination.icon),
              label: destination.label,
            ),
        ],
      ),
    );
  }
}

class _NavDestination {
  const _NavDestination(this.label, this.icon, this.path);

  final String label;
  final IconData icon;
  final String path;
}
