import 'package:flutter/material.dart';

import '../widgets/feature_placeholder.dart';

class GovernmentDashboardScreen extends StatelessWidget {
  const GovernmentDashboardScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return const FeaturePlaceholder(
      title: 'Government institution dashboard',
      description: 'This protected placeholder establishes the dashboard route '
          'for verified institution users.',
      actions: [
        'Route guard requires an authenticated Supabase session.',
        'Institution verification and role checks will be layered on this route.',
        'Assigned report queues and AI chat will be built in later Person 1 tasks.',
      ],
    );
  }
}
