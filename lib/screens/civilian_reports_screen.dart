import 'package:flutter/material.dart';

import '../widgets/feature_placeholder.dart';

class CivilianReportsScreen extends StatelessWidget {
  const CivilianReportsScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return const FeaturePlaceholder(
      title: 'Civilian report intake',
      description: 'This protected route is ready for the verified resident '
          'report form in the next Person 2 task.',
      actions: [
        'Route guard requires an authenticated Supabase session.',
        'Screen is reserved for category, image, description, and location input.',
        'Submission will later write reports and uploaded images to Supabase.',
      ],
    );
  }
}
