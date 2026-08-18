namespace assetlen.Service.DbServices.SmtpClient
{
    /// <summary>
    /// Architectural Calm, for email (CLAUDE.md §2).
    /// <para>
    /// Every transactional email is assembled from this one shell, because the
    /// four that existed before it had drifted into four different products: a
    /// purple gradient, a pink one, a green one, and a fourth purple that had
    /// already diverged from the first. A reader who has just been asked for a
    /// code is the least equipped to tell a real message from a forged one, so
    /// the mail carrying the code has to look unmistakably like the application.
    /// </para>
    /// <para>
    /// <b>The wordmark is a constant, not configuration.</b> It used to be read
    /// from <c>SmtpSettings:CompanyName</c>, which still held the value belonging
    /// to the project this codebase was forked from — so ASSETLEN users received
    /// one-time passwords signed "Ministry of Works &amp; Transport". An identity
    /// that can be wrong in a config file will eventually be wrong in a config
    /// file, and on this particular email being wrong looks like phishing.
    /// </para>
    /// <para>
    /// Email constraints, not preferences: layout is tables because Outlook uses
    /// a word-processor engine with no flexbox; every style is inline because
    /// several clients drop a <c>&lt;style&gt;</c> block; colours are literal hex
    /// because custom properties do not resolve. That is why the tokens below are
    /// constants and not <c>var(--al-*)</c> — this is the one place in the
    /// codebase where §2.1's "always go through a token" cannot be obeyed
    /// literally, so it is obeyed here instead.
    /// </para>
    /// </summary>
    internal static class EmailTheme
    {
        // ── Palette (CLAUDE.md §2.1, light column) ──────────────────────────
        // The hairlines are the rgba tokens flattened onto white: mail clients
        // give no compositing guarantees, so they are resolved once, here.
        public const string Paper          = "#f4f3ee";
        public const string Surface        = "#ffffff";
        public const string Text           = "#1a1d21";
        public const string Muted          = "#6b6f76";
        public const string Subtle         = "#9ca0a7";
        public const string Hairline       = "#e8e6e0";
        public const string HairlineStrong = "#d9d7d1";
        public const string Accent         = "#c2542a";
        public const string AccentSoft     = "#f3dcd0";
        public const string Warning        = "#c69430";
        public const string WarningSoft    = "#f7efdc";

        // ── Type (CLAUDE.md §2.2) ───────────────────────────────────────────
        // Webfonts do not load in most mail clients, so each stack names a
        // fallback that is actually installed and carries the same voice.
        public const string Display = "'Fraunces', Georgia, 'Times New Roman', serif";
        public const string Body    = "'Inter', 'Segoe UI', Helvetica, Arial, sans-serif";
        public const string Mono    = "'JetBrains Mono', 'SFMono-Regular', Consolas, Menlo, monospace";

        public const string ProductName = "ASSETLEN";
        public const string Website     = "https://assetlen.com";

        /// <summary>
        /// The outer frame: warm ground, one white sheet, hairline rules, and the
        /// wordmark in the display serif. No gradient and no coloured banner — the
        /// accent is spent on the one thing the reader opened the mail for.
        /// </summary>
        /// <param name="preheader">
        /// The line the inbox shows beside the subject. Left unset it fills with
        /// whatever text comes first, which on a code email is the code itself —
        /// putting a live one-time password on the lock screen of anyone holding
        /// the phone.
        /// </param>
        internal static string Shell(string preheader, string eyebrow, string title, string bodyHtml) =>
$@"<!DOCTYPE html>
<html lang='en'>
<head>
<meta charset='utf-8'>
<meta name='viewport' content='width=device-width, initial-scale=1'>
<meta name='color-scheme' content='light'>
<title>{Escape(title)}</title>
</head>
<body style='margin:0; padding:0; background-color:{Paper}; -webkit-text-size-adjust:100%;'>

<div style='display:none; max-height:0; overflow:hidden; opacity:0; color:transparent; font-size:1px; line-height:1px;'>{Escape(preheader)}</div>

<table role='presentation' cellpadding='0' cellspacing='0' border='0' width='100%' style='background-color:{Paper};'>
<tr>
<td align='center' style='padding:32px 16px;'>

  <table role='presentation' cellpadding='0' cellspacing='0' border='0' width='600' style='width:600px; max-width:100%; background-color:{Surface}; border:1px solid {Hairline};'>

    <tr>
      <td style='padding:28px 40px 22px 40px; border-bottom:1px solid {Hairline};'>
        <table role='presentation' cellpadding='0' cellspacing='0' border='0' width='100%'>
          <tr>
            <td align='left' style='font-family:{Display}; font-size:19px; font-weight:600; letter-spacing:0.09em; color:{Text};'>{ProductName}</td>
            <td align='right' style='font-family:{Body}; font-size:10px; font-weight:600; letter-spacing:0.14em; text-transform:uppercase; color:{Subtle};'>{Escape(eyebrow)}</td>
          </tr>
        </table>
      </td>
    </tr>

    <tr>
      <td style='padding:36px 40px 8px 40px;'>
        <h1 style='margin:0; font-family:{Display}; font-size:26px; line-height:1.25; font-weight:600; color:{Text};'>{Escape(title)}</h1>
      </td>
    </tr>

    <tr>
      <td style='padding:0 40px 36px 40px;'>{bodyHtml}</td>
    </tr>

    <tr>
      <td style='padding:22px 40px 30px 40px; border-top:1px solid {Hairline}; background-color:{Paper};'>
        <p style='margin:0 0 6px 0; font-family:{Body}; font-size:12px; line-height:1.6; color:{Muted};'>
          This message was sent automatically. Replies to it are not read.
        </p>
        <p style='margin:0; font-family:{Body}; font-size:12px; line-height:1.6; color:{Subtle};'>
          <a href='{Website}' style='color:{Muted}; text-decoration:underline;'>assetlen.com</a>
          &nbsp;&middot;&nbsp; &copy; {DateTime.UtcNow.Year} {ProductName}
        </p>
      </td>
    </tr>

  </table>

</td>
</tr>
</table>

</body>
</html>";

        /// <summary>
        /// The code itself. Mono, tabular, widely tracked, on an accent-tinted
        /// panel. The reader is copying six digits off one screen while holding a
        /// second device, so here legibility outranks restraint.
        /// </summary>
        internal static string CodePanel(string code, int expiryMinutes) =>
$@"        <table role='presentation' cellpadding='0' cellspacing='0' border='0' width='100%' style='margin:26px 0;'>
          <tr>
            <td align='center' style='padding:26px 20px; background-color:{AccentSoft}; border:1px solid {HairlineStrong};'>
              <div style='font-family:{Mono}; font-size:34px; font-weight:700; letter-spacing:0.22em; color:{Accent};'>{Escape(code)}</div>
              <div style='margin-top:12px; font-family:{Body}; font-size:12px; letter-spacing:0.06em; color:{Muted};'>Expires in {expiryMinutes} minutes</div>
            </td>
          </tr>
        </table>";

        /// <summary>A quiet advisory. An amber hairline on a tinted ground, never a red alarm for something that is usually nothing.</summary>
        internal static string Notice(string bodyHtml) =>
$@"        <table role='presentation' cellpadding='0' cellspacing='0' border='0' width='100%' style='margin:24px 0 0 0;'>
          <tr>
            <td style='padding:14px 18px; background-color:{WarningSoft}; border-left:3px solid {Warning};'>
              <p style='margin:0; font-family:{Body}; font-size:13px; line-height:1.6; color:{Text};'>{bodyHtml}</p>
            </td>
          </tr>
        </table>";

        internal static string Paragraph(string html) =>
$@"        <p style='margin:0 0 16px 0; font-family:{Body}; font-size:15px; line-height:1.65; color:{Muted};'>{html}</p>";

        /// <summary>A single terracotta action, table-wrapped so Outlook honours its padding.</summary>
        internal static string Button(string href, string label) =>
$@"        <table role='presentation' cellpadding='0' cellspacing='0' border='0' style='margin:26px 0;'>
          <tr>
            <td align='center' style='background-color:{Accent};'>
              <a href='{href}' style='display:inline-block; padding:14px 30px; font-family:{Body}; font-size:15px; font-weight:600; color:#ffffff; text-decoration:none;'>{Escape(label)}</a>
            </td>
          </tr>
        </table>";

        /// <summary>The URL in full, for a client that will not render the button.</summary>
        internal static string FallbackLink(string href) =>
$@"        <p style='margin:0 0 16px 0; font-family:{Body}; font-size:12px; line-height:1.6; color:{Subtle}; word-break:break-all;'>
          If the button does not work, paste this into your browser:<br>
          <a href='{href}' style='color:{Accent}; text-decoration:underline;'>{Escape(href)}</a>
        </p>";

        /// <summary>
        /// A display name reaches this markup from user input, so it is escaped
        /// rather than trusted. Codes and URLs go through the same door.
        /// </summary>
        internal static string Escape(string? value) => System.Net.WebUtility.HtmlEncode(value ?? string.Empty);
    }
}
