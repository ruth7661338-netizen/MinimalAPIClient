import React, { useState } from "react";
import service from "./service";

function Register({ onSuccess, goToLogin }) {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");

  async function handleSubmit(e) {
    e.preventDefault();
    setError("");
    try {
      await service.register(username, password);
      onSuccess();
    } catch (err) {
      if (err.response && err.response.status === 409) {
        setError("שם המשתמש כבר קיים");
      } else {
        setError("ההרשמה נכשלה, נסי שוב");
      }
    }
  }

  return (
    <form onSubmit={handleSubmit} dir="rtl" style={{ maxWidth: 300, margin: "80px auto" }}>
      <h2>הרשמה</h2>
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
      <button type="submit">הרשמי</button>
      {error && <p style={{ color: "red" }}>{error}</p>}
      <p>
        כבר יש לך חשבון?{" "}
        <button type="button" onClick={goToLogin}>להתחברות</button>
      </p>
    </form>
  );
}

export default Register;