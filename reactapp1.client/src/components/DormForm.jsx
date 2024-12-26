import React, { useState } from 'react';
import axios from 'axios';

const DormForm = () => {
    const [name, setName] = useState('');

    const handleSubmit = async (e) => {
        e.preventDefault();
        console.log("Form Submitted, dorm name: ", name);

        if (!name) {
            alert('Įrašykite bendrabučio pavadinimą');
            return;
        }

        try {
            const token = localStorage.getItem('authToken');
            const response = await axios.post('/api/dorms',
                { name },
                {
                    headers: {
                        Authorization: `Bearer ${token}`
                    }
                }
            );

            console.log('Response:', response);

            if (response.status === 400) {
                alert('Įsitikinite, kad visi laukai užpildyti');
            } else if (response.status === 422) {
                alert('Įsitikinkite, kad bendrabučio pavadinimas yra tinkamo ilgio');
            } else if (response.status === 201) {
                alert('Sėkmingai sukurtas');
            } else {
                alert('Error: ' + response.status);
            }
        } catch (error) {
            console.error('Error creating dorm: ', error);
            if (error.response && error.response.status === 401) {
                alert('Neturite tam skirtų teisių');
            } else {
                alert('Error creating dorm');
            }
        }
    };


    return (
        <form onSubmit={handleSubmit}>
            <label>
                Dorm Name:
                <input
                    type="text"
                    value={name}
                    onChange={(e) => setName(e.target.value)}
                />
            </label>
            <button type="submit">Create Dorm</button>
        </form>
    );
};

export default DormForm;
