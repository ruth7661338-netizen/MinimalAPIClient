import React, { useState } from "react";
import App from "./App";
import Login from "./Login";
import Register from "./Register";
import service from "./service";

function AuthGate() {
  const [loggedIn, setLoggedIn] = useState(!!localStorage.getItem("token"));
  const [page, setPage] = useState("login");

  if (loggedIn) {
    return (
      <>
        <button
          onClick={() => {
            service.logout();
            setLoggedIn(false);
          }}
        >
          התנתקות
        </button>
        <App />
      </>
    );
  }

  if (page === "register") {
    return (
      <Register
        onSuccess={() => setLoggedIn(true)}
        goToLogin={() => setPage("login")}
      />
    );
  }

  return (
    <Login
      onSuccess={() => setLoggedIn(true)}
      goToRegister={() => setPage("register")}
    />
  );
}

export default AuthGate;