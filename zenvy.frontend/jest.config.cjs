module.exports = {
  testEnvironment: 'jsdom',
  setupFilesAfterEnv: ['<rootDir>/src/test/setupTests.js'],
  transform: { '^.+\\.[jt]sx?$': 'babel-jest' },
  testMatch: ['<rootDir>/src/**/*.test.{js,jsx}'],
  clearMocks: true,
};
