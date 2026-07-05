import 'package:flutter/material.dart';

import 'routes/app_router.dart';
import 'theme/app_theme.dart';

class UrbanfixApp extends StatelessWidget {
  const UrbanfixApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp.router(
      title: 'urbanfix',
      debugShowCheckedModeBanner: false,
      theme: AppTheme.light(),
      routerConfig: appRouter,
    );
  }
}
