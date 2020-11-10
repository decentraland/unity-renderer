import React from "react";
import { Spinner } from "../common/Spinner";
import { LoginHeader } from "./LoginHeader";
import "./InitialLoading.css";

export const InitialLoading: React.FC = () => (
  <div className="initialLoading">
    <LoginHeader />
    <Spinner />
  </div>
);
