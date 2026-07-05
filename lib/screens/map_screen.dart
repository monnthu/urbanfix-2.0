import 'package:flutter/material.dart';

import '../widgets/feature_placeholder.dart';

class MapScreen extends StatelessWidget {
  const MapScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return const FeaturePlaceholder(
      title: 'Public report map',
      description: 'The public route is open now so Flutter Web can verify '
          'routing before map packages and report markers are added.',
      actions: [
        'Unauthenticated visitors can reach this page.',
        'Reserved for OpenStreetMap markers, filters, and report details.',
        'Later reads should use non-sensitive public report fields only.',
      ],
    );
  }
}
