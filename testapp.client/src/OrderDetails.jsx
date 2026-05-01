import { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { orderSchema } from './schemas/orderSchema';
const defaultObj = {
    senderCity: 'Город отправителя',
    senderAddress: 'Адрес отправителя',
    recieverCity: 'Город получателя',
    recieverAddress: 'Адрес получателя',
    weight: 5,
};
const OrderDetails = ({ onSubmitSuccess, initialData, isReadOnly = false }) => {

    const [error, setError] = useState(null);

    const { register, reset, handleSubmit, formState: { errors, isSubmitting } } = useForm({
        resolver: zodResolver(orderSchema),
        defaultValues: defaultObj
    });

    useEffect(() => {
        const formattedData = {
            ... (initialData ?? defaultObj),
            dateOfPicking: initialData?.dateOfPicking
                ? new Date(initialData.dateOfPicking).toISOString().split('T')[0]
                : new Date(2026, 5, 3).toISOString().split('T')[0]
        };
        reset(formattedData);
    }, [initialData, reset]);

    const onSubmit = async (data) => {
        if (isReadOnly) return;

        const query = `api/orders/create`;

        const response = await fetch(query, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data),
        });

        if (response.ok) {
            onSubmitSuccess();
        } else {
            setError(await response.json());
        }
    };

    return (
        <form onSubmit={handleSubmit(onSubmit)} className="order-form">
            <h3>{isReadOnly ? 'Просмотр заказа' : 'Оформление заказа'}</h3>

            <div>
                <label>Город отправителя</label>
                <input
                    {...register("senderCity")}
                    readOnly={isReadOnly}
                    className={isReadOnly ? 'input-readonly' : ''}
                />
                {errors.senderCity && <p className="error">{errors.senderCity.message}</p>}
            </div>

            <div>
                <label>Адрес отправителя</label>
                <input
                    {...register("senderAddress")}
                    readOnly={isReadOnly}
                    className={isReadOnly ? 'input-readonly' : ''}
                />
                {errors.senderAddress && <p className="error">{errors.senderAddress.message}</p>}
            </div>

            <div>
                <label>Город получателя</label>
                <input
                    {...register("recieverCity")}
                    readOnly={isReadOnly}
                    className={isReadOnly ? 'input-readonly' : ''}
                />
                {errors.recieverCity && <p className="error">{errors.recieverCity.message}</p>}
            </div>

            <div>
                <label>Адрес получателя</label>
                <input
                    {...register("recieverAddress")}
                    readOnly={isReadOnly}
                    className={isReadOnly ? 'input-readonly' : ''}
                />
                {errors.recieverAddress && <p className="error">{errors.recieverAddress.message}</p>}
            </div>

            <div>
                <label>Вес (кг)</label>
                <input
                    type="number"
                    {...register("weight", { valueAsNumber: true })}
                    readOnly={isReadOnly}
                />
                {errors.weight && <p className="error">{errors.weight.message}</p>}
            </div>

            <div>
                <label>Дата забора</label>

                <input
                    type="date"
                    {...register("dateOfPicking")}
                    readOnly={isReadOnly}
                />
                {errors.dateOfPicking && <p className="error">{errors.dateOfPicking.message}</p>}
            </div>

            {!isReadOnly && (
                <button type="submit" disabled={isSubmitting}>
                    {isSubmitting ? 'Отправка...' : 'Создать заказ'}
                </button>
            )}

            {error && (
                <p>{typeof error === 'string' ? error : JSON.stringify(error)}</p>
            )}
        </form>
    );
};

export default OrderDetails;