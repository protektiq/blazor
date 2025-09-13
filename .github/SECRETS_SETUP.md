# GitHub Secrets Setup Guide

This guide helps you configure the required secrets for the GitHub Actions workflows.

## Required Secrets

### 1. Vercel Deployment Secrets

**VERCEL_TOKEN**
- Go to [Vercel Dashboard](https://vercel.com/account/tokens)
- Click "Create Token"
- Name: "GitHub Actions"
- Scope: Full Account
- Copy the token and add as `VERCEL_TOKEN`

**VERCEL_ORG_ID**
- Go to [Vercel Dashboard](https://vercel.com/account)
- Copy your Team ID from the URL or settings
- Add as `VERCEL_ORG_ID`

**VERCEL_PROJECT_ID**
- Go to your project in Vercel Dashboard
- Copy the Project ID from the URL or settings
- Add as `VERCEL_PROJECT_ID`

### 2. Azure Deployment Secrets (Optional)

**AZURE_CREDENTIALS**
```json
{
  "clientId": "your-client-id",
  "clientSecret": "your-client-secret",
  "subscriptionId": "your-subscription-id",
  "tenantId": "your-tenant-id"
}
```

**AZURE_APP_NAME**
- Your Azure App Service name
- Example: "customer-support-api"

**AZURE_PUBLISH_PROFILE**
- Download from Azure App Service → Get Publish Profile
- Add the entire XML content as `AZURE_PUBLISH_PROFILE`

### 3. SonarCloud Secret (Optional)

**SONAR_TOKEN**
- Go to [SonarCloud](https://sonarcloud.io/account/security/)
- Generate a new token
- Add as `SONAR_TOKEN`

### 4. Notification Secrets (Optional)

**SLACK_WEBHOOK**
- Create a Slack app and get webhook URL
- Add as `SLACK_WEBHOOK`

## How to Add Secrets

1. Go to your GitHub repository
2. Click **Settings** → **Secrets and variables** → **Actions**
3. Click **New repository secret**
4. Enter the secret name and value
5. Click **Add secret**

## Secret Validation

You can validate your secrets by running the workflows manually:

1. Go to **Actions** tab in your repository
2. Select a workflow (e.g., "Deploy to Vercel")
3. Click **Run workflow**
4. Check the logs for any authentication errors

## Security Best Practices

- ✅ Use least-privilege tokens
- ✅ Rotate tokens regularly
- ✅ Use environment-specific secrets
- ❌ Never commit secrets to code
- ❌ Don't share secrets in logs or screenshots

## Troubleshooting

### Common Issues

**"Authentication failed"**
- Verify the secret name matches exactly
- Check if the token has expired
- Ensure the token has correct permissions

**"Resource not found"**
- Verify resource IDs are correct
- Check if resources exist in the correct region/account

**"Permission denied"**
- Verify the service principal has correct roles
- Check if the token has necessary scopes

### Testing Secrets Locally

You can test some secrets locally:

```bash
# Test Vercel token
vercel --token $VERCEL_TOKEN projects list

# Test Azure credentials
az login --service-principal --username $CLIENT_ID --password $CLIENT_SECRET --tenant $TENANT_ID
```

## Environment-Specific Configuration

For different environments, you can use different secrets:

- `VERCEL_TOKEN_STAGING`
- `VERCEL_TOKEN_PRODUCTION`
- `AZURE_CREDENTIALS_STAGING`
- `AZURE_CREDENTIALS_PRODUCTION`

Update the workflows to use environment-specific secrets as needed.
