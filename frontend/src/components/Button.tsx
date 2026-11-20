import type { ButtonHTMLAttributes } from 'react'

const variants = {
  primary: 'bg-ink text-white hover:bg-ink-soft',
  accent: 'bg-accent text-ink hover:bg-accent-strong',
  ghost: 'bg-transparent text-ink hover:bg-ink/5',
} as const

type Props = ButtonHTMLAttributes<HTMLButtonElement> & {
  variant?: keyof typeof variants
  busy?: boolean
}

export function Button({ variant = 'primary', busy = false, disabled, className = '', children, ...rest }: Props) {
  return (
    <button
      {...rest}
      disabled={disabled || busy}
      aria-busy={busy || undefined}
      className={`inline-flex min-h-11 items-center justify-center gap-2 rounded-lg px-4 text-sm font-semibold transition-colors disabled:cursor-not-allowed disabled:opacity-60 ${variants[variant]} ${className}`}
    >
      {busy && <span className="size-4 animate-spin rounded-full border-2 border-current border-t-transparent" aria-hidden />}
      {children}
    </button>
  )
}
