import { Router } from "express";
import { createMessageController, getMessagesController } from "../controllers/message.controllers";

const router = Router();

router.get("/", getMessagesController);

router.post("/", createMessageController);

export default router;