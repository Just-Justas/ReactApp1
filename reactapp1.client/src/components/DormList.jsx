import React, { useState, useEffect } from 'react';
import axios from 'axios';
import Modal from './ModalDormUpdate';
import { useNavigate } from 'react-router-dom';

const DormList = () => {
    const [dorms, setDorms] = useState([]);
    const [loading, setLoading] = useState(true);
    const [showModal, setShowModal] = useState(false);
    const [selectedDormId, setSelectedDormId] = useState(null);
    const token = localStorage.getItem('accessToken');

    useEffect(() => {
        axios.get('/api/dorms')
            .then(response => {
                setDorms(response.data);
                setLoading(false);
            })
            .catch(error => {
                console.error("Error fetching dorms:", error);
                setLoading(false);
            });
    }, []);

    const handleEdit = (dormId) => {
        setSelectedDormId(dormId);
        setShowModal(true);
    };

    const navigate = useNavigate();

    const handleClick = () => {
        navigate('/dormsCreate');
    };

    const handleSaveDorm = async (dormId, updatedDorm) => {
        try {
            const response = await axios.put(`/api/dorms/${dormId}`, updatedDorm, {
                headers: {
                    Authorization: `Bearer ${token}`,
                },
            });
            if (response.status === 200) {
                setDorms(dorms.map(dorm => dorm.id === dormId ? { ...dorm, ...updatedDorm } : dorm));
                alert('Sėkmingai atnaujinta!');
            }
            setShowModal(false);
        } catch (error) {
            console.error('Problema atnaujinant bendrabutį:', error);
            alert('Error updating dorm');
        }
    };

    const handleDelete = async (dormId) => {
        const confirmed = window.confirm("Ar tikrai norite pašalinti šį bendrabutį?");
        if (confirmed) {
            try {
                const response = await axios.delete(`/api/dorms/${dormId}`, {
                    headers: {
                        Authorization: `Bearer ${token}`,
                    },
                });
                if (response.status === 200) {
                    setDorms(dorms.filter(dorm => dorm.id !== dormId));
                    alert('Dorm deleted successfully.');
                }
            } catch (error) {
                console.error("Error deleting dorm:", error);
                alert("Error deleting dorm.");
            }
        }
    };

    if (loading) return <p>Loading dorms...</p>;

    return (
        <div className="container">
            <h2>Bendrabučiai</h2>
            {dorms.length === 0 ? (
                <p>Nėra bendrabučių.</p>
            ) : (
                <table className="table table-striped">
                    <thead>
                        <tr>
                            <th scope="col">Bendrabučio pavadinimas</th>
                            <th scope="col">Keisti</th>
                            <th scope="col">Trinti</th>
                            <th scope="col">Gyventojai</th>
                            <th scope="col">Įrašai</th>
                        </tr>
                    </thead>
                    <tbody>
                        {dorms.map(dorm => (
                            <tr key={dorm.id}>
                                <td>{dorm.name}</td>
                                <td>
                                    <button onClick={() => handleEdit(dorm.id)} className="btn btn-primary">
                                        Keisti
                                    </button>
                                </td>
                                <td>
                                    <button onClick={() => handleDelete(dorm.id)} className="btn btn-danger">
                                        Trinti
                                    </button>
                                </td>
                                <td>
                                    <button
                                        onClick={() => navigate(`/dorms/${dorm.id}/residents`)}
                                        className="btn btn-link text-white"
                                    >
                                        Gyventojai
                                    </button>
                                </td>
                                <td>
                                    <button
                                        onClick={() => navigate(`/dorms/${dorm.id}/posts`)}
                                        className="btn btn-link text-white"
                                    >
                                        Įrašai
                                    </button>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            )}
            <button onClick={handleClick} className="btn btn-success">
                Pridėti bendrabūtį
            </button>

            <Modal
                showModal={showModal}
                onClose={() => setShowModal(false)}
                dormId={selectedDormId}
                onSave={handleSaveDorm}
            />
        </div>
    );
};

export default DormList;
