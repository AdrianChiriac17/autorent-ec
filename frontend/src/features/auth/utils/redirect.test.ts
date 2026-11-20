import { describe, expect, it } from 'vitest'
import { redirectTarget } from './redirect'

describe('redirectTarget', () => {
  it('returns the in-app page that required login', () => {
    expect(redirectTarget({ from: '/admin/users' })).toBe('/admin/users')
  })

  it.each([null, undefined, {}, { from: 42 }, { from: 'https://evil.example' }, { from: '//evil.example' }])(
    'falls back to the home page for %j',
    (state) => {
      expect(redirectTarget(state)).toBe('/')
    },
  )
})
