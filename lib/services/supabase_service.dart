import 'package:supabase_flutter/supabase_flutter.dart';

class SupabaseService {
  const SupabaseService._();

  static bool _isConfigured = false;

  static bool get isConfigured => _isConfigured;

  static SupabaseClient? get client {
    if (!_isConfigured) {
      return null;
    }

    return Supabase.instance.client;
  }

  static Session? get currentSession => client?.auth.currentSession;

  static Future<void> initialize() async {
    const supabaseUrl = String.fromEnvironment('SUPABASE_URL');
    const supabaseAnonKey = String.fromEnvironment('SUPABASE_ANON_KEY');

    if (supabaseUrl.isEmpty || supabaseAnonKey.isEmpty) {
      _isConfigured = false;
      return;
    }

    await Supabase.initialize(
      url: supabaseUrl,
      publishableKey: supabaseAnonKey,
    );
    _isConfigured = true;
  }
}
