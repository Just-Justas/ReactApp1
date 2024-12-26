import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { useParams } from 'react-router-dom';

const ResidentManagementPage = () => {
    const { dormId } = useParams();
    const [residents, setResidents] = useState([]);
    const [newResident, setNewResident] = useState({
        Firstname: '',
        Lastname: '',
        RoomNumber: ''
    });
    const [loading, setLoading] = useState(true);
    const [editingResident, setEditingResident] = useState(null);

    useEffect(() => {
        axios.get(`/api/dorms/${dormId}/residents`)
            .then(response => {
                setResidents(response.data);
                setLoading(false);
            })
            .catch(error => {
                console.error("Error:", error);
                setLoading(false);
            });
    }, [dormId]);

    const handleAddResident = () => {
        const token = localStorage.getItem('accessToken');

        if (!token) {
            return;
        }
        axios.post(`/api/dorms/${dormId}/residents`, newResident, {
            headers: {
                Authorization: `Bearer ${token}`,
            },
        })
            .then(response => {
                setResidents([...residents, response.data]);
                setNewResident({ Firstname: '', Lastname: '', RoomNumber: '' });
            })
            .catch(error => {
                console.error("Error adding resident:", error);
            });
    };

    const handleDeleteResident = (residentId) => {
        const token = localStorage.getItem('accessToken');
        axios.delete(`/api/dorms/${dormId}/residents/${residentId}`, {
            headers: {
                Authorization: `Bearer ${token}`,
            },
        })
            .then(() => {
                setResidents(residents.filter(resident => resident.id !== residentId));
            })
            .catch(error => {
                console.error("Error deleting resident:", error);
            });
    };

    const handleSaveEdit = () => {
        const token = localStorage.getItem('accessToken');
        axios.put(`/api/dorms/${dormId}/residents/${editingResident.id}`, editingResident, {
            headers: {
                Authorization: `Bearer ${token}`,
            },
        })
            .then(() => {
                setResidents(residents.map(resident =>
                    resident.id === editingResident.id ? editingResident : resident
                ));
                setEditingResident(null);
            })
            .catch(error => {
                console.error("Error saving changes:", error);
            });
    };

    if (loading) return <p>Loading residents...</p>;

    return (
        <div className="container">
            <h2>Bendrabutis nr. {dormId}</h2>

            <h3>Resident List</h3>
            <table className="table">
                <thead>
                    <tr>
                        <th>#</th>
                        <th>Vardas</th>
                        <th>Pavardė</th>
                        <th>Kambarys</th>
                        <th>Veiksmai</th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td>New</td>
                        <td>
                            <input
                                type="text"
                                placeholder="First Name"
                                value={newResident.Firstname}
                                onChange={(e) => setNewResident({ ...newResident, Firstname: e.target.value })}
                                className="form-control"
                            />
                        </td>
                        <td>
                            <input
                                type="text"
                                placeholder="Last Name"
                                value={newResident.Lastname}
                                onChange={(e) => setNewResident({ ...newResident, Lastname: e.target.value })}
                                className="form-control"
                            />
                        </td>
                        <td>
                            <input
                                type="number"
                                placeholder="Room Number"
                                value={newResident.RoomNumber}
                                onChange={(e) => setNewResident({ ...newResident, RoomNumber: e.target.value })}
                                className="form-control"
                            />
                        </td>
                        <td>
                            <button onClick={handleAddResident} className="btn btn-primary btn-sm">Add</button>
                        </td>
                    </tr>

                    {residents.map((resident, index) => (
                        <tr key={resident.id}>
                            <td>{index + 1}</td>
                            <td>
                                {editingResident?.id === resident.id ? (
                                    <input
                                        type="text"
                                        value={editingResident.firstname}
                                        onChange={(e) =>
                                            setEditingResident({ ...editingResident, firstname: e.target.value })
                                        }
                                        className="form-control"
                                    />
                                ) : (
                                    resident.firstname
                                )}
                            </td>
                            <td>
                                {editingResident?.id === resident.id ? (
                                    <input
                                        type="text"
                                        value={editingResident.lastname}
                                        onChange={(e) =>
                                            setEditingResident({ ...editingResident, lastname: e.target.value })
                                        }
                                        className="form-control"
                                    />
                                ) : (
                                    resident.lastname
                                )}
                            </td>
                            <td>
                                {editingResident?.id === resident.id ? (
                                    <input
                                        type="number"
                                        value={editingResident.roomNumber}
                                        onChange={(e) =>
                                            setEditingResident({ ...editingResident, roomNumber: e.target.value })
                                        }
                                        className="form-control"
                                    />
                                ) : (
                                    resident.roomNumber
                                )}
                            </td>
                            <td>
                                {editingResident?.id === resident.id ? (
                                    <>
                                        <button onClick={handleSaveEdit} className="btn btn-success btn-sm me-2">
                                            Save
                                        </button>
                                        <button onClick={() => setEditingResident(null)} className="btn btn-secondary btn-sm">
                                            Cancel
                                        </button>
                                    </>
                                ) : (
                                    <>
                                        <button
                                            onClick={() => setEditingResident(resident)}
                                            className="btn btn-primary btn-sm me-2"
                                        >
                                            Edit
                                        </button>
                                        <button
                                            onClick={() => handleDeleteResident(resident.id)}
                                            className="btn btn-danger btn-sm"
                                        >
                                            Delete
                                        </button>
                                    </>
                                )}
                            </td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
};

export default ResidentManagementPage;
