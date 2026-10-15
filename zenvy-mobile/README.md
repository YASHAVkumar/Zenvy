# Zenvy Mobile (React Native / Expo / JavaScript)

This is a **mobile conversion starter** based on the supplied Zenvy React web frontend. It uses JavaScript and JSX only.

## Included
- Native sign-in screen calling `POST /api/v1/auth/login`
- Token persistence using AsyncStorage
- Bottom-tab navigation: Dashboard, Products, Operations, Profile
- Executive dashboard calling `GET /api/v1/dashboards/executive`
- Products list starter calling `GET /api/v1/products`
- Shared native components and styles

## Important: configure the API address first

Edit `src/config.js` and replace `http://YOUR_PC_LAN_IP:5066` with an address reachable from your phone.

- Do **not** use `localhost` or `127.0.0.1` on a physical phone: those addresses point to the phone itself.
- For local testing, use your Windows PC's LAN IPv4 address and the actual port your API listens on, e.g. `http://192.168.1.20:5066`.
- Your phone and PC need a network path to each other. Windows Firewall and ASP.NET Core binding settings may need adjustment.
- For production, use your deployed HTTPS API URL.
- iOS may block plain HTTP requests depending on app/network configuration; HTTPS is recommended.

## Install and run

Use Node.js LTS. From this folder:

```powershell
npm install
npx expo install --fix
npx expo start --clear
```

Scan the QR code in Expo Go. If the dependency checker changes package versions, accept compatible versions and restart the server.

## What's not yet ported

The uploaded React web app has many pages and browser-specific components (Admin, Finance, Compensation, Profit Distribution, Sales Desk, Payments, Workflow, Workspace, Manual, etc.). This starter provides a mobile shell and begins the conversion of login, executive dashboard and products. It does **not** claim every web page or every workflow has been converted. The Products endpoint `/api/v1/products` is a best-effort starter assumption; confirm it against your backend routes.

The original web frontend uses `localStorage`, `window` events, React Router, `lucide-react`, HTML forms/tables and browser CSS. These are not directly reusable as-is in React Native. Auth persistence has been moved to AsyncStorage, and navigation uses React Navigation.

## JavaScript-only
All app source files are `.js`; no `.ts` or `.tsx` files are included.
