import { z } from 'zod';

export const orderSchema = z.object({
    senderCity: z.string().min(2, "Минимум 2 символа"),
    senderAddress: z.string().min(5, "Слишком короткий адрес"),
    recieverCity: z.string().min(2, "Минимум 2 символа"),
    recieverAddress: z.string().min(5, "Слишком короткий адрес"),
    weight: z.number().positive("Вес должен быть больше 0").max(250, "Максимальный вес - 250 кг."),
    dateOfPicking: z.string().refine((val) => !isNaN(Date.parse(val)), "Неверная дата")
});
