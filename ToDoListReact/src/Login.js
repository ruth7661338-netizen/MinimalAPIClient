import React, { useState } from "react";
import service from "./service";

function Login({ onSuccess, goToRegister }) {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");

  async function handleSubmit(e) {
    e.preventDefault();
    setError("");
    try {
      await service.login(username, password);
      onSuccess();
    } catch (err) {
      setError("שם משתמש או סיסמה שגויים");
    }
  }

  return (
    <form onSubmit={handleSubmit} dir="rtl" style={{ maxWidth: 300, margin: "80px auto" }}>
      <h2>התחברות</h2>
      <input
        placeholder="שם משתמש"
        value={username}
        onChange={(e) => setUsername(e.target.value)}
        required
      />
      <br /><br />
      <input
        type="password"
        placeholder="סיסמה"
        value={password}
        onChange={(e) => setPassword(e.target.value)}
        required
      />
      <br /><br />
      <button type="submit">התחברי</button>
      {error && <p style={{ color: "red" }}>{error}</p>}
      <p>
        אין לך חשבון?{" "}
        <button type="button" onClick={goToRegister}>להרשמה</button>
      </p>
    </form>
  );
}

export default Login;