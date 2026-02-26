import express from 'express';
import cors from 'cors';
import messageRoutes from './routes/messages.routes';
//import { errorHandler } from './middlewares/errorHandler';

const app = express();

const corsOptions = {
    origin: '*',
    allowedHeaders: ['Content-Type', 'x-api-key'],
    methods: ['GET', 'POST', 'PUT', 'DELETE', 'OPTIONS'],
};

app.use(cors(corsOptions));
app.use(express.json());

// Routes
app.use('/messages', messageRoutes);

// Global error handler (should be after routes)
//app.use(errorHandler);

export default app;