import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:supabase_flutter/supabase_flutter.dart';

import '../routes/app_router.dart';
import '../services/supabase_service.dart';

class LoginScreen extends StatelessWidget {
  const LoginScreen({
    this.redirectPath,
    super.key,
  });

  final String? redirectPath;

  @override
  Widget build(BuildContext context) {
    final isConfigured = SupabaseService.isConfigured;

    return Center(
      child: ConstrainedBox(
        constraints: const BoxConstraints(maxWidth: 520),
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Card(
            child: Padding(
              padding: const EdgeInsets.all(24),
              child: Column(
                mainAxisSize: MainAxisSize.min,
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    'Sign in to urbanfix',
                    style: Theme.of(context).textTheme.headlineSmall,
                  ),
                  const SizedBox(height: 12),
                  Text(
                    isConfigured
                        ? 'Google OAuth and TOTP enrollment will be completed '
                            'in the next task. Protected routes already send '
                            'unauthenticated users here.'
                        : 'Add Supabase environment values before enabling '
                            'Google OAuth and MFA.',
                  ),
                  const SizedBox(height: 24),
                  FilledButton.icon(
                    onPressed: isConfigured ? _signInWithGoogle : null,
                    icon: const Icon(Icons.login),
                    label: const Text('Continue with Google'),
                  ),
                  const SizedBox(height: 12),
                  TextButton(
                    onPressed: () => context.go(AppRoute.landing),
                    child: const Text('Back to landing page'),
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }

  Future<void> _signInWithGoogle() async {
    final targetPath = redirectPath ?? AppRoute.reports;

    await SupabaseService.client?.auth.signInWithOAuth(
      OAuthProvider.google,
      redirectTo: '${Uri.base.origin}$targetPath',
    );
  }
}
