import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import '../routes/app_router.dart';

class LandingScreen extends StatelessWidget {
  const LandingScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final textTheme = Theme.of(context).textTheme;

    return SingleChildScrollView(
      padding: const EdgeInsets.all(24),
      child: Center(
        child: ConstrainedBox(
          constraints: const BoxConstraints(maxWidth: 960),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const SizedBox(height: 32),
              Text(
                'Report civic issues. Track public progress.',
                style: textTheme.displaySmall?.copyWith(
                  fontWeight: FontWeight.bold,
                ),
              ),
              const SizedBox(height: 16),
              Text(
                'urbanfix connects residents with verified local institutions '
                'through simple mobile reporting, a public issue map, and '
                'government-only follow-up tools.',
                style: textTheme.titleLarge,
              ),
              const SizedBox(height: 32),
              Wrap(
                spacing: 12,
                runSpacing: 12,
                children: [
                  FilledButton.icon(
                    onPressed: () => context.go(AppRoute.reports),
                    icon: const Icon(Icons.add_location_alt_outlined),
                    label: const Text('Start a report'),
                  ),
                  OutlinedButton.icon(
                    onPressed: () => context.go(AppRoute.map),
                    icon: const Icon(Icons.map_outlined),
                    label: const Text('View public map'),
                  ),
                ],
              ),
              const SizedBox(height: 40),
              GridView.count(
                crossAxisCount: MediaQuery.sizeOf(context).width > 720 ? 3 : 1,
                crossAxisSpacing: 16,
                mainAxisSpacing: 16,
                shrinkWrap: true,
                physics: const NeverScrollableScrollPhysics(),
                children: const [
                  _ValueCard(
                    title: 'Civilian reports',
                    body: 'Residents submit location, category, description, '
                        'and image evidence after verification.',
                  ),
                  _ValueCard(
                    title: 'Public accountability',
                    body: 'A map and support votes help communities understand '
                        'which issues need attention.',
                  ),
                  _ValueCard(
                    title: 'Institution workflow',
                    body: 'Verified government users receive routed reports '
                        'and manage follow-up in a protected dashboard.',
                  ),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _ValueCard extends StatelessWidget {
  const _ValueCard({
    required this.title,
    required this.body,
  });

  final String title;
  final String body;

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(title, style: Theme.of(context).textTheme.titleLarge),
            const SizedBox(height: 12),
            Text(body),
          ],
        ),
      ),
    );
  }
}
