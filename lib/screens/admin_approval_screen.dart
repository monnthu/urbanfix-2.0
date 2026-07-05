import 'package:flutter/material.dart';

import '../widgets/feature_placeholder.dart';

class AdminApprovalScreen extends StatelessWidget {
  const AdminApprovalScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return const FeaturePlaceholder(
      title: 'Admin institution approval',
      description: 'This protected route is ready for manual review of '
          'institution registrations and proof documents.',
      actions: [
        'Route guard requires an authenticated Supabase session.',
        'Future admin role checks can be added without changing public routes.',
        'Approval actions will update institution verification status.',
      ],
    );
  }
}
