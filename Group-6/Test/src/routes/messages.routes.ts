import { Router } from "express";
import { messageReceived } from "../controllers/message.controllers";

const router = Router();

router.post("/message", messageReceived);

export default router;