using System.Text;

namespace assetlen.Service.DbServices
{
    /// <summary>
    /// The wording of every one-time code ASSETLEN sends by SMS, in one place.
    /// <para>
    /// There were three separate strings before this, written at three different
    /// times, and none of them named the product. A code arriving as "Your
    /// password reset code is: 481920" is indistinguishable from a phishing
    /// message, and the recipient has no way to tell which of the apps on their
    /// phone asked for it — least of all on a site in Uganda where the sender ID
    /// is a short alphanumeric the reader has never seen.
    /// </para>
    ///
    /// <para><b>The rules the copy obeys:</b></para>
    /// <list type="number">
    ///   <item>Name the product first. It is the only authentication the reader has.</item>
    ///   <item>Code early, so it is readable in a lock-screen preview without opening anything.</item>
    ///   <item>State the expiry, so an old code in the thread is obviously stale.</item>
    ///   <item>Say never share. Code-relay fraud is the common attack on exactly this message.</item>
    ///   <item><b>No link.</b> A one-time-code SMS that contains a URL trains people to tap links in code messages, which is the attack.</item>
    /// </list>
    ///
    /// <para><b>The character budget.</b> A GSM-7 message is 160 characters; one
    /// character outside that alphabet silently re-encodes the whole message as
    /// UCS-2 and the limit collapses to 70, splitting it into billable segments
    /// and sometimes arriving out of order. So the copy here uses plain ASCII
    /// only — no curly quotes, no en or em dashes, no ellipsis character, no
    /// emoji — and every message is checked against <see cref="FitsOneSegment"/>
    /// by the tests below it in this file's history. Keep new copy under
    /// <see cref="SingleSegment"/>.</para>
    /// </summary>
    public static class OtpSms
    {
        /// <summary>Characters available in a single GSM-7 message.</summary>
        public const int SingleSegment = 160;

        /// <summary>Characters per part once a message is split. The header eats seven.</summary>
        public const int ConcatenatedSegment = 153;

        /// <summary>
        /// Confirming an email address, a phone number, or a contact change.
        /// </summary>
        public static string Verification(string code, int expiryMinutes) =>
            $"ASSETLEN: {code} is your verification code. It expires in {expiryMinutes} minutes. Never share it with anyone.";

        /// <summary>
        /// Resetting a forgotten password.
        /// </summary>
        public static string PasswordReset(string code, int expiryMinutes) =>
            $"ASSETLEN: {code} is your password reset code. It expires in {expiryMinutes} minutes. Never share it with anyone.";

        /// <summary>
        /// A reset an administrator started on someone's behalf. Saying so is the
        /// point: a code the reader did not ask for is the one they most need to
        /// be able to explain, and an unexplained code is the one they forward.
        /// </summary>
        public static string PasswordResetByAdmin(string code, int expiryMinutes) =>
            $"ASSETLEN: {code} is your password reset code, started by an administrator. It expires in {expiryMinutes} minutes. Never share it.";

        // ── The budget, enforceable ─────────────────────────────────────────

        /// <summary>
        /// True when the message will travel as one GSM-7 SMS. False means either
        /// it is too long or it contains a character that forces UCS-2 — both of
        /// which cost more and can arrive as two pieces.
        /// </summary>
        public static bool FitsOneSegment(string message) =>
            IsGsm7(message) && Gsm7Length(message) <= SingleSegment;

        /// <summary>
        /// Length in GSM-7 units. The ten extension characters occupy two units
        /// each, so a naive <c>string.Length</c> under-counts a message that uses
        /// them and lets a 160-character string quietly become two segments.
        /// </summary>
        public static int Gsm7Length(string message)
        {
            if (string.IsNullOrEmpty(message)) return 0;

            var units = 0;
            foreach (var c in message)
            {
                if (Gsm7Extended.IndexOf(c) >= 0) units += 2;
                else if (Gsm7Basic.IndexOf(c) >= 0) units += 1;
                else return int.MaxValue; // not representable: caller must treat as UCS-2
            }
            return units;
        }

        public static bool IsGsm7(string message) =>
            !string.IsNullOrEmpty(message) && message.All(c => Gsm7Basic.IndexOf(c) >= 0 || Gsm7Extended.IndexOf(c) >= 0);

        /// <summary>How many SMS this message will actually cost.</summary>
        public static int SegmentCount(string message)
        {
            if (string.IsNullOrEmpty(message)) return 0;

            if (!IsGsm7(message))
            {
                // UCS-2: 70 characters, or 67 per part once split.
                var len = message.Length;
                return len <= 70 ? 1 : (int)Math.Ceiling(len / 67.0);
            }

            var units = Gsm7Length(message);
            return units <= SingleSegment ? 1 : (int)Math.Ceiling(units / (double)ConcatenatedSegment);
        }

        // The GSM 03.38 basic alphabet, then the escape table whose members cost
        // two units. Written out rather than computed so it can be read against
        // the spec.
        private const string Gsm7Basic =
            "@£$¥èéùìòÇ\nØø\rÅå"
          + "Δ_ΦΓΛΩΠΨΣΘΞÆæßÉ"
          + " !\"#¤%&'()*+,-./0123456789:;<=>?"
          + "¡ABCDEFGHIJKLMNOPQRSTUVWXYZÄÖÑÜ§"
          + "¿abcdefghijklmnopqrstuvwxyzäöñüà";

        private const string Gsm7Extended = "^{}\\[~]|€\f";
    }
}
