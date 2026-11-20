import { useState, type FormEvent } from 'react'
import { Link, Navigate, useNavigate } from 'react-router'
import { Alert } from '../../../components/Alert'
import { Button } from '../../../components/Button'
import { TextField } from '../../../components/TextField'
import { ApiError } from '../../../lib/http'
import { AuthCard } from '../components/AuthCard'
import { useAuth } from '../hooks/useAuth'

export function RegisterPage() {
  const { state, register } = useAuth()
  const navigate = useNavigate()
  const [form, setForm] = useState({ fullName: '', email: '', phone: '', password: '' })
  const [error, setError] = useState<ApiError | null>(null)
  const [submitting, setSubmitting] = useState(false)

  if (state.status === 'authenticated') {
    return <Navigate to="/" replace />
  }

  const update = (field: keyof typeof form) => (e: React.ChangeEvent<HTMLInputElement>) =>
    setForm((current) => ({ ...current, [field]: e.target.value }))

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setSubmitting(true)
    setError(null)
    try {
      await register({ ...form, phone: form.phone.trim() || undefined })
      navigate('/', { replace: true })
    } catch (err) {
      setError(err instanceof ApiError ? err : new ApiError(0, 'unknown', 'Something went wrong.'))
    } finally {
      setSubmitting(false)
    }
  }

  const fieldError = (name: string) => error?.fieldErrors[name]?.[0]
  const hasFieldErrors = error !== null && Object.keys(error.fieldErrors).length > 0

  return (
    <AuthCard
      title="Create your account"
      subtitle="Book cars in Craiova in a few clicks."
      footer={
        <>
          Already have an account?{' '}
          <Link to="/login" className="font-semibold text-ink underline underline-offset-4">
            Log in
          </Link>
        </>
      }
    >
      <form onSubmit={handleSubmit} noValidate className="space-y-4">
        {error && !hasFieldErrors && <Alert>{error.message}</Alert>}
        <TextField label="Full name" autoComplete="name" required value={form.fullName} onChange={update('fullName')} error={fieldError('fullName')} />
        <TextField label="Email" type="email" autoComplete="email" required value={form.email} onChange={update('email')} error={fieldError('email')} />
        <TextField label="Phone (optional)" type="tel" autoComplete="tel" value={form.phone} onChange={update('phone')} error={fieldError('phone')} />
        <TextField
          label="Password"
          type="password"
          autoComplete="new-password"
          required
          minLength={8}
          value={form.password}
          onChange={update('password')}
          error={fieldError('password')}
          hint="At least 8 characters."
        />
        <Button type="submit" busy={submitting} className="w-full">
          Create account
        </Button>
      </form>
    </AuthCard>
  )
}
