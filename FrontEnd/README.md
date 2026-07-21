# ShiftOne Admin

ShiftOne Admin is the React management console for the ShiftOne ASP.NET Core API.

## Requirements

- Node.js compatible with Create React App 5
- The ShiftOne API running locally or at the URL configured in `.env`

## Configuration

`.env` supports:

```env
GENERATE_SOURCEMAP=false
REACT_APP_API_URL=http://localhost:5234
REACT_APP_NAME=ShiftOne
```

## Scripts

```bash
npm install
npm start
npm run build
```

## Application Routes

- `/auth/sign-in` for authentication
- `/admin/default` for the dashboard
- `/admin/data-tables` for users management
- `/admin/profile` for the authenticated profile
