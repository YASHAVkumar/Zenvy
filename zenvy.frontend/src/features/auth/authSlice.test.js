import authSlice, { logout, setAuth, setError, setLoading } from './authSlice';

const authReducer = authSlice.reducer;
const nameIdentifier = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier';
const nameClaim = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name';
const roleClaim = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
const createToken = (claims) => `header.${btoa(JSON.stringify(claims))}.signature`;

const signedOutState = {
  token: null,
  user: null,
  isAuthenticated: false,
  loading: false,
  error: null,
};

describe('authSlice', () => {
  it('stores a successful login and clears transient state', () => {
    const state = authReducer({ ...signedOutState, loading: true, error: 'Previous error' }, setAuth({
      token: createToken({
        [nameIdentifier]: '1', [nameClaim]: 'Asha', email: 'asha@example.com', [roleClaim]: ' Admin ',
      }),
      user: { userId: 'spoofed', fullName: 'Not from token', role: 'Manager' },
    }));

    expect(state).toEqual({
      token: expect.any(String),
      user: { userId: '1', fullName: 'Asha', email: 'asha@example.com', role: 'Admin', roles: ['Admin'] },
      isAuthenticated: true,
      loading: false,
      error: null,
    });
  });

  it('tracks loading and request errors independently', () => {
    const loadingState = authReducer(signedOutState, setLoading(true));
    const errorState = authReducer(loadingState, setError('Invalid credentials'));

    expect(errorState.loading).toBe(true);
    expect(errorState.error).toBe('Invalid credentials');
  });

  it('removes in-memory authentication details on logout', () => {
    const state = authReducer({
      token: createToken({ [roleClaim]: 'Admin' }), user: { role: 'Admin' }, isAuthenticated: true, loading: true, error: 'error',
    }, logout());

    expect(state).toEqual(signedOutState);
  });
});
