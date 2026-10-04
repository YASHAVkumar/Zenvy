const claimNames = {
  userId: [
    'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier',
    'sub',
    'nameid',
    'userId',
  ],
  fullName: [
    'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name',
    'name',
    'fullName',
  ],
  email: [
    'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress',
    'email',
  ],
  roles: [
    'http://schemas.microsoft.com/ws/2008/06/identity/claims/role',
    'role',
    'roles',
  ],
};

const getClaim = (claims, names) => {
  for (const name of names) {
    const value = claims[name];
    if (value !== undefined && value !== null) return value;
  }
  return null;
};

export const decodeJwtPayload = (token) => {
  try {
    const payloadPart = token?.split('.')[1];
    if (!payloadPart) return null;

    const base64 = payloadPart.replace(/-/g, '+').replace(/_/g, '/');
    const paddedBase64 = base64.padEnd(Math.ceil(base64.length / 4) * 4, '=');
    const binary = atob(paddedBase64);
    const utf8 = Array.from(binary, (character) => `%${character.charCodeAt(0).toString(16).padStart(2, '0')}`).join('');
    return JSON.parse(decodeURIComponent(utf8));
  } catch {
    return null;
  }
};

export const getUserFromAccessToken = (token) => {
  const claims = decodeJwtPayload(token);
  if (!claims || typeof claims !== 'object') return null;

  const rawRoles = getClaim(claims, claimNames.roles);
  const roles = (Array.isArray(rawRoles) ? rawRoles : [rawRoles])
    .map((role) => String(role || '').trim())
    .filter(Boolean);
  const userId = getClaim(claims, claimNames.userId);
  const fullName = getClaim(claims, claimNames.fullName);
  const email = getClaim(claims, claimNames.email);

  if (!userId && !fullName && !email && roles.length === 0) return null;

  return {
    userId: userId ? String(userId) : '',
    fullName: fullName ? String(fullName) : '',
    email: email ? String(email) : '',
    role: roles[0] || '',
    roles,
  };
};

export const hasAnyRole = (userRoles, allowedRoles) => {
  const roles = (Array.isArray(userRoles) ? userRoles : [userRoles])
    .map((role) => String(role || '').trim().toLowerCase())
    .filter(Boolean);

  return allowedRoles.some((allowedRole) => roles.includes(String(allowedRole).trim().toLowerCase()));
};
