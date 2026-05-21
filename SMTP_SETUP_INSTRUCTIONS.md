# SMTP Configuration Instructions - Brevo (formerly Sendinblue)

## Why Brevo?

Brevo offers an excellent free tier for development and small-scale production:

- **300 emails per day FREE** (forever)
- Easy to set up
- Reliable delivery
- Simple upgrade path when you need more emails
- Professional email templates support
- Excellent deliverability rates

## Setup Instructions

### Step 1: Create a Brevo Account

1. Go to [https://www.brevo.com](https://www.brevo.com)
2. Click "Sign up free"
3. Fill in your details:
   - Email address
   - Password
   - Company name (you can use "MoWT" or your organization name)
4. Verify your email address by clicking the link sent to your inbox

### Step 2: Get Your SMTP Credentials

1. Log in to your Brevo dashboard
2. Click on your name (top right) → **SMTP & API**
3. In the SMTP section, you'll see:
   - **SMTP Server:** `smtp-relay.brevo.com`
   - **Port:** `587` (TLS) or `465` (SSL)
   - **Login:** Your Brevo account email
   - **SMTP Key:** Click "Generate a new SMTP key" or "Create a new SMTP key"

4. **IMPORTANT:** Copy and save your SMTP key immediately - you won't be able to see it again!

### Step 3: Configure Your Application

Update your `appsettings.json` file with your Brevo credentials:

```json
"SmtpSettings": {
  "Host": "smtp-relay.brevo.com",
  "Port": 587,
  "SenderEmail": "noreply@yourdomain.com",
  "SenderName": "MoWT Digital Library",
  "Username": "your-brevo-email@example.com",
  "Password": "your-brevo-smtp-key-here",
  "ReplyToEmail": "support@yourdomain.com",
  "CompanyName": "Ministry of Works & Transport",
  "WebsiteLink": "https://mowt.com"
}
```

**Replace these values:**

- `SenderEmail`: Your verified sender email (see Step 4)
- `Username`: Your Brevo account email
- `Password`: The SMTP key you generated in Step 2
- `ReplyToEmail`: Where users' replies should go
- `CompanyName` and `WebsiteLink`: Your organization details

### Step 4: Verify Your Sender Email (Required)

Before you can send emails, you must verify your sender email address:

1. In Brevo dashboard, go to **Senders, Domains & Dedicated IPs** → **Senders**
2. Click **Add a sender**
3. Enter your email (e.g., `noreply@yourdomain.com`)
4. Enter sender name (e.g., "MoWT Digital Library")
5. Click **Add**
6. Check your inbox for verification email from Brevo
7. Click the verification link

**Note:** If you don't own a domain yet, you can use your Brevo account email as the sender email for testing.

### Step 5: (Optional) Verify Your Domain for Better Deliverability

For production use, it's recommended to verify your domain:

1. Go to **Senders, Domains & Dedicated IPs** → **Domains**
2. Click **Authenticate a domain**
3. Enter your domain (e.g., `yourdomain.com`)
4. Follow the instructions to add DNS records (SPF, DKIM, DMARC)
5. Wait for DNS propagation (can take up to 48 hours)

This improves email deliverability and reduces the chance of emails going to spam.

### Step 6: Test Your Configuration

1. Start your application
2. Try to register a new user
3. Check that you receive the verification email
4. Check Brevo dashboard → **Statistics** → **Email** to see delivery status

## Free Tier Limits

- **300 emails/day** (forever free)
- Unlimited contacts
- All email features included
- Brevo logo in emails (removed in paid plans)

## Upgrade Options (When You Need More)

- **Lite Plan:** $25/month - 10,000 emails/month (no daily limit)
- **Starter Plan:** $35/month - 20,000 emails/month
- **Business Plan:** $65/month - 40,000 emails/month
- Custom plans available for higher volumes

## Alternative Free SMTP Services

If you prefer other options:

### 1. **Gmail SMTP** (Free)

- Limit: 100-500 emails/day
- Host: `smtp.gmail.com`
- Port: `587`
- Requires "App Password" if 2FA enabled

### 2. **Mailgun** (Free)

- Limit: 5,000 emails/month for 3 months, then paid
- Good API documentation

### 3. **SendGrid** (Free)

- Limit: 100 emails/day forever free
- Good for testing

### 4. **Elastic Email** (Free)

- Limit: 100 emails/day forever free

## Troubleshooting

### Emails not sending?

1. Check SMTP credentials are correct
2. Verify sender email is verified in Brevo
3. Check application logs for error messages
4. Verify port 587 is not blocked by firewall

### Emails going to spam?

1. Verify your domain (DNS records)
2. Use a professional sender email (not @gmail.com)
3. Avoid spam trigger words in subject lines
4. Keep email content professional

### Rate limit exceeded?

- Monitor your daily usage in Brevo dashboard
- Consider upgrading if you consistently hit 300/day
- Implement email batching if sending bulk emails

## Security Best Practices

1. **Never commit SMTP credentials to Git**
   - Use environment variables or Azure Key Vault in production
   - Add `appsettings.json` to `.gitignore` if it contains credentials

2. **Use User Secrets for development:**

   ```bash
   dotnet user-secrets init --project assetlen.API
   dotnet user-secrets set "SmtpSettings:Password" "your-smtp-key" --project assetlen.API
   ```

3. **Use Azure App Configuration or Key Vault in production**

## Email Sending Implementation

The application sends emails asynchronously (fire-and-forget) to avoid blocking user registration:

```csharp
// Registration workflow:
1. User submits registration form
2. User account created in database
3. Verification code generated and stored
4. Email sending queued in background thread
5. User sees success message immediately
6. Email arrives within seconds
```

This ensures fast user experience even if email sending is slow.

## Support

- **Brevo Documentation:** [https://help.brevo.com](https://help.brevo.com)
- **Brevo Support:** Available via dashboard chat
- For application issues: Check logs in `assetlen.API` project

---

**Last Updated:** February 16, 2026
