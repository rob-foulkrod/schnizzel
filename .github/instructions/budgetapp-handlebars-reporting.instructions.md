---
description: "Use when creating or editing Handlebars templates for HTML reports, including .hbs, .handlebars, and HTML templates with {{ }} placeholders. Covers template safety, structure, and reporting markup conventions."
name: "BudgetApp Handlebars Reporting Conventions"
applyTo: ["**/*.hbs", "**/*.handlebars", "**/*.hbs.html", "**/*.handlebars.html"]
---
# BudgetApp Handlebars Reporting Conventions

- Treat these as default requirements for report templates unless the user asks otherwise.
- Keep business logic out of templates. Perform calculations, filtering, and aggregation in C# before rendering.
- Use semantic HTML for report output: `header`, `main`, `section`, `table`, `thead`, `tbody`, `tfoot`, and `caption` where appropriate.
- Escape dynamic content by default with `{{value}}`. Use unescaped output (`{{{value}}}`) only for trusted, pre-sanitized HTML fragments.
- Prefer small, focused partials for reusable blocks such as headers, summary cards, transaction rows, and footers.
- Keep control flow simple and readable. Avoid deeply nested `if` and `each` blocks.
- Assume missing or null values can occur. Render safe fallbacks (for example `N/A` or `0.00`) instead of empty or broken markup.
- Use stable, testable formatting conventions for dates and currency by relying on explicit helper usage rather than implicit locale defaults.
- Preserve accessible table structure for screen readers and exports, including header cells (`th`) and scope attributes when needed.
- Keep CSS hooks predictable with class names intended for reporting (`report-*`, `summary-*`, `transactions-*`).
- When template behavior changes, update or add tests that validate rendered output snippets and key placeholders.