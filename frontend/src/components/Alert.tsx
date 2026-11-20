import type { ReactNode } from 'react'

const tones = {
  error: 'border-danger/30 bg-danger/5 text-danger',
  info: 'border-line bg-white text-ink-soft',
} as const

export function Alert({ tone = 'error', children }: { tone?: keyof typeof tones; children: ReactNode }) {
  return (
    <div role={tone === 'error' ? 'alert' : 'status'} className={`rounded-lg border px-4 py-3 text-sm ${tones[tone]}`}>
      {children}
    </div>
  )
}
