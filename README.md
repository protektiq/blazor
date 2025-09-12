# Customer Support System

A modern customer support system built with Blazor WebAssembly and ASP.NET Core Web API.

## Features

- **Dashboard**: Overview of ticket statistics and recent activity
- **Ticket Management**: Create, view, and manage support tickets
- **Comments System**: Add comments to tickets with internal/external visibility
- **Responsive Design**: Works on desktop, tablet, and mobile devices
- **Real-time Updates**: Built with SignalR for live updates

## Architecture

- **Frontend**: Blazor WebAssembly (deployed to Vercel)
- **Backend**: ASP.NET Core Web API
- **Database**: SQLite (can be easily switched to PostgreSQL/Supabase)
- **Authentication**: ASP.NET Core Identity

## Deployment to Vercel

This project is configured for deployment to Vercel as a static site.

### Prerequisites

1. Install the Vercel CLI:
   ```bash
   npm i -g vercel
   ```

2. Make sure you have .NET 8 SDK installed

### Deploy Steps

1. **Login to Vercel**:
   ```bash
   vercel login
   ```

2. **Deploy the application**:
   ```bash
   vercel --prod
   ```

3. **Configure environment variables** (if needed):
   - Go to your Vercel dashboard
   - Navigate to your project settings
   - Add any required environment variables

### Local Development

1. **Run the API** (in one terminal):
   ```bash
   cd CustomerSupportSystem.Api
   dotnet run
   ```

2. **Run the WebAssembly app** (in another terminal):
   ```bash
   cd CustomerSupportSystem.Wasm
   dotnet run
   ```

3. **Run tests**:
   ```bash
   dotnet test
   ```

## Project Structure

```
├── CustomerSupportSystem.Domain/     # Domain entities and models
├── CustomerSupportSystem.Data/       # Data access layer and DbContext
├── CustomerSupportSystem.Api/        # Web API backend
├── CustomerSupportSystem.Wasm/       # Blazor WebAssembly frontend
├── CustomerSupportSystem.Tests/      # Playwright tests
├── vercel.json                       # Vercel deployment configuration
└── package.json                      # Node.js configuration for Vercel
```

## API Endpoints

- `GET /api/tickets` - Get all tickets
- `GET /api/tickets/{id}` - Get specific ticket
- `GET /api/tickets/{id}/comments` - Get ticket comments
- `POST /api/tickets` - Create new ticket
- `PUT /api/tickets/{id}` - Update ticket
- `POST /api/tickets/{id}/comments` - Add comment to ticket

## Testing

The project includes comprehensive Playwright tests covering:
- Navigation and routing
- Ticket management workflows
- Responsive design
- Error handling
- End-to-end user journeys

Run tests with:
```bash
dotnet test
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Submit a pull request

## License

This project is licensed under the MIT License.
