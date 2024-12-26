import React, { useState } from 'react';
import axios from 'axios';

const CreateResident = () => {
    const [firstname, setFirstname] = useState('');
    const [lastname, setLastname] = useState('');
    const [roomNumber, setRoomNumber] = useState('');
    const [dormId, setDormId] = useState('');
    const [error, setError] = useState('');
    const [success, setSuccess] = useState(false);

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');
        setSuccess(false);
        if (!firstname || !lastname || !roomNumber || !dormId) {
            setError('Please provide all required fields.');
            return;
        }

        const token = localStorage.getItem('authToken');
        if (!token) {
            setError('You must be logged in.');
            return;
        }

        try {
            const response = await axios.post(
                '/api/resident',
                {
                    firstname,
                    lastname,
                    roomNumber: parseInt(roomNumber),
                    dormId: parseInt(dormId)
                },
                {
                    headers: {
                        Authorization: `Bearer ${token}`,
                    },
                }
            );

            if (response.status === 201) {
                setSuccess(true);
                setFirstname('');
                setLastname('');
                setRoomNumber('');
                setDormId('');
            }
        } catch (err) {
            console.error('Error creating resident:', err);
            setError('Failed to create resident. Please try again.');
        }
    };

    return (
        <div>
            <h2>Pridėti gyventoją</h2>
            <form onSubmit={handleSubmit}>
                <div>
                    <label>
                        Vardas:
                        <input
                            type="text"
                            value={firstname}
                            onChange={(e) => setFirstname(e.target.value)}
                            required
                        />
                    </label>
                </div>
                <div>
                    <label>
                        Pavardė:
                        <input
                            type="text"
                            value={lastname}
                            onChange={(e) => setLastname(e.target.value)}
                            required
                        />
                    </label>
                </div>
                <div>
                    <label>
                        Kambarys:
                        <input
                            type="number"
                            value={roomNumber}
                            onChange={(e) => setRoomNumber(e.target.value)}
                            required
                        />
                    </label>
                </div>
                <div>
                    <label>
                        Bendrabučio ID:
                        <input
                            type="number"
                            value={dormId}
                            onChange={(e) => setDormId(e.target.value)}
                            required
                        />
                    </label>
                </div>
                {error && <div style={{ color: 'red' }}>{error}</div>}
                {success && <div style={{ color: 'green' }}>Gyventojas sėkmingai pridėtas!</div>}
                <button type="submit">Pridėti gyventoją</button>
            </form>
        </div>
    );
};

export default CreateResident;
