import { getUserFromAccessToken, hasAnyRole } from './tokenClaims';

const nameIdentifier = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier';
const nameClaim = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name';
const roleClaim = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
const createToken = (claims) => `header.${btoa(unescape(encodeURIComponent(JSON.stringify(claims))))}.signature`;

describe('access token claims', () => {
  it('decodes identity and role claims from a URL-safe JWT payload', () => {
    const token = createToken({
      [nameIdentifier]: 'u-1',
      [nameClaim]: 'Élodie Example',
      [roleClaim]: [' Admin ', 'Manager'],
    }).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');

    expect(getUserFromAccessToken(token)).toEqual({
      userId: 'u-1',
      fullName: 'Élodie Example',
      email: '',
      role: 'Admin',
      roles: ['Admin', 'Manager'],
    });
  });

  it('returns no identity for a malformed token', () => {
    expect(getUserFromAccessToken('not-a-jwt')).toBeNull();
  });

  it('matches role names without casing or whitespace differences', () => {
    expect(hasAnyRole([' TeamLead ', 'Accountant'], ['admin', 'teamlead'])).toBe(true);
    expect(hasAnyRole('SalesPerson', ['Admin', 'Manager'])).toBe(false);
  });
});
