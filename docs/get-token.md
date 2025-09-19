# Guide: Get your SubX-Api Token

Follow these steps to generate a token and use it in the SubdivX plugin for Emby.

1) Go to https://subx-api.duckdns.org/
2) Register or sign in.
3) Click “New API Key”.
4) Enter a recognizable name (e.g., "Emby"). Leave expiration blank or set it to 0 if you don’t want to renew it later.
5) Create the key. Copy the API key right now: it won’t be shown again.
6) In Emby, paste the value into the SubdivX plugin setting field `SubX Api Token` and save.

Tips
- Keep your API key secret. Treat it like a password.
- If you lose it, revoke the key and create a new one.
- If you get 401/403 errors, verify the token value and that it hasn’t expired.

