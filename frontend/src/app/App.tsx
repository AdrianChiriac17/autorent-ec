import { Link, Route, Routes } from 'react-router'
import { AdminUsersPage } from '../features/admin/pages/AdminUsersPage'
import { RequireAuth } from '../features/auth/components/RequireAuth'
import { LoginPage } from '../features/auth/pages/LoginPage'
import { RegisterPage } from '../features/auth/pages/RegisterPage'
import { AppLayout } from './AppLayout'
import { HomePage } from './HomePage'

export function App() {
  return (
    <Routes>
      <Route element={<AppLayout />}>
        <Route index element={<HomePage />} />
        <Route path="login" element={<LoginPage />} />
        <Route path="register" element={<RegisterPage />} />
        <Route
          path="admin/users"
          element={
            <RequireAuth admin>
              <AdminUsersPage />
            </RequireAuth>
          }
        />
        <Route path="*" element={<NotFound />} />
      </Route>
    </Routes>
  )
}

function NotFound() {
  return (
    <div className="mx-auto max-w-md px-4 py-16 text-center">
      <h1 className="font-display text-2xl font-bold">Page not found</h1>
      <Link to="/" className="mt-4 inline-block font-semibold underline underline-offset-4">
        Back to the home page
      </Link>
    </div>
  )
}
