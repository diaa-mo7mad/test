import { Route, Routes } from "react-router-dom"
import Fortget from "./pages/forgot_password/Forget"
import Register from "./pages/regester/Regester"
import Sign from "./pages/signin/Sign"
import DashBoard from "./pages/dashboard/Dashboard"
import Irrigation from "./pages/irrigation/Irrigation"
import Analytics from "./pages/analytics/Analytics "
import Settings from "./pages/settings/Settings"
import Domes from "./pages/domes/Domes"
import ProtectedRoute from "./component/ProtectedRoute"

function App() {
  return (
    <Routes>
      <Route path="/" element={<Sign />} />
      <Route path="/Regester" element={<Register />} />
      <Route path="/Forget" element={<Fortget />} />

      <Route path="/dashboard" element={<ProtectedRoute><DashBoard /></ProtectedRoute>} />
      <Route path="/Analytics" element={<ProtectedRoute><Analytics /></ProtectedRoute>} />
      <Route path="/Irrigation" element={<ProtectedRoute><Irrigation /></ProtectedRoute>} />
      <Route path="/Settings" element={<ProtectedRoute><Settings /></ProtectedRoute>} />
      <Route path="/Domes" element={<ProtectedRoute><Domes /></ProtectedRoute>} />
    </Routes>
  )
}

export default App;
