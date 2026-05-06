import { useEffect, useState, useRef } from 'react';
import './App.css';
import OrderDetails from './OrderDetails';

function App() {
    const [data, setData] = useState();
    const [selectedOrder, setSelectedOrder] = useState(null);
    const [isReadOnly, setIsReadOnly] = useState(false);
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [page, setPage] = useState({ limit: 10, lastId : [], current : 0, hasMore: false });
    const [loadError, setLoadError] = useState(null);
    const timerRef = useRef(null);

    const handleShowDetails = async (id) => {

        let query = `api/orders/${id}`;
        const response = await fetch(query);

        if (response.ok) {

            setIsReadOnly(true);
            const order = await response.json();
            setSelectedOrder(order);
            setIsModalOpen(true);
        }
    };

    const handleShowCreateModal = () => {
        setSelectedOrder(null);
        setIsReadOnly(false);
        setIsModalOpen(true);
    }

    const closeModal = () => {
        setIsModalOpen(false);
        setSelectedOrder(null);
    };

    const loadNext = async () => {
        await populateData(page.current + 1)
    }

    const loadPrev = async () => {
        await populateData(page.current - 1)
    }

    const onSubmit = async () => {
        closeModal();
        await populateData();
    }

    const handleUpdate = async () => {
        if (timerRef.current) {
            clearTimeout(timerRef.current);
        }
        await populateData();
    }

    const handleLimitChanged = (e) => {

        if (timerRef.current) {
            clearTimeout(timerRef.current);
        }
        const newValue = e.target.value;

        if (newValue <= 0) {
            return;
        }

        const newPage = { ...page, limit: parseInt(newValue) }

        setPage(newPage)

        timerRef.current = setTimeout(async () => {



            await populateData(page.current, newPage.limit);
            timerRef.current = null;
        }, 500);
    }

    useEffect(() => {

        const start = async () => {
            let backoff = 1000;

            for (var i = 0; i < 2; i++) {
                let result = await populateData();

                if (result)
                    return;

                await new Promise(res => setTimeout(res, backoff));
                backoff *= 2;
            }

        }

        timerRef.current = setTimeout(async () => {
            await start();
            timerRef.current = null;
        }, 1000);
    }, []);

    const contents = data === undefined
        ? <p><em>Loading...</em></p>
        : <table className="table table-striped" style={{ marginTop : '50px' }} aria-labelledby="tableLabel">
            <thead>
                <tr>
                    <th>Номер заказа</th>
                    <th>Город отправителя</th>
                    <th>Адрес отправителя</th>
                    <th>Город получателя</th>
                    <th>Адрес получателя</th>
                    <th>Вес посылки</th>
                    <th>Дата забора груза</th>
                    <th></th>
                </tr>
            </thead>
            <tbody>
                {data.map(model =>
                    <tr key={model.id}>
                        <td>{model.id}</td>
                        <td>{model.senderCity}</td>
                        <td>{model.senderAddress}</td>
                        <td>{model.recieverCity}</td>
                        <td>{model.recieverAddress}</td>
                        <td>{model.weight}</td>
                        <td>
                            {new Date(model.dateOfPicking).toISOString().split('T')[0]}
                        </td>
                        <td>
                            <button className="" onClick={() => handleShowDetails(model.id)}>Просмотр</button>
                        </td>
                    </tr>
                )}
            </tbody>
        </table>;
        

    return (
        <div>
            <h1 id="tableLabel">Таблица заказов</h1>        

            <div className="control-panel">
                <label>Число элементов: </label>
                <input type="number" value={page.limit} onChange={handleLimitChanged}  />
                <button disabled={page.current === 0} onClick={loadPrev}>Предыдущие записи</button>
                <button disabled={!page.hasMore} onClick={loadNext}>Следующие записи</button>
                <button onClick={handleShowCreateModal}>Новая запись</button>
                <button onClick={handleUpdate}>Обновить</button>
            </div>

            {loadError?.error == null ? contents : 
                
                <p>{loadError.error}</p>
            }

            {
                isModalOpen && (
                    <div className="modalOverlay">
                        <div>
                            <button onClick={closeModal} style={{ float: 'right' }}>Закрыть</button>
                            <OrderDetails onSubmitSuccess={onSubmit} initialData={selectedOrder} isReadOnly={isReadOnly} />
                        </div>
                    </div>
                )
            }
        </div>
    );

    async function onSuccessLoaded(response, pageNumber, limit) {

        const data = await response.json();  
        
        if(data.data.length > 0)
        {
            const id = data.data.at(-1).id;

            let arr = page.lastId;
            let end = pageNumber > page.current ? undefined : pageNumber - page.current - 1;

            arr = [...arr.slice(0, end), id]
            const newPage = { limit: limit, current: pageNumber, lastId: arr, hasMore: data.hasMore }
            setPage(newPage)
            setData(data.data);
        }
        else {
            setLoadError({
                error: "Нет данных",
            })
        }
    }

    async function populateData(pageNumber = null, limit = null) {

        let query = 'api/orders'

        limit ??= page.limit
        pageNumber ??= page.current

        const params = new URLSearchParams({
            limit: limit.toString()
        });

        if (pageNumber > 0)
            params.append('lastId', page.lastId[pageNumber - 1].toString())

        let response = null;

        try {

            response = await fetch(`${query}?${params}`);
            if (!response.ok) {
                throw new Error(`Ошибка сервера: ${response.status}`);
            }

            setLoadError(null);
            onSuccessLoaded(response, pageNumber, limit);
            return true;            
        }
        catch (error) {
            setLoadError({
                error: error.message,
                status: response.status
            })
            return false;
        }
        
    }
}

export default App;