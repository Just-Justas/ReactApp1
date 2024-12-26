import React, { useState, useEffect } from 'react';
import axios from 'axios';
import PropTypes from 'prop-types';


const ModalDormUpdate = ({ showModal, onClose, dormId, onSave }) => {
    const [dormName, setDormName] = useState('');
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        if (dormId) {
            axios.get(`/api/dorms/${dormId}`)
                .then(response => {
                    setDormName(response.data.name);
                    setLoading(false);
                })
                .catch(error => {
                    console.error("Error fetching dorm details:", error);
                    setLoading(false);
                });
        }
    }, [dormId]);

    const handleSave = () => {
        if (dormName.trim()) {
            const updatedDorm = { name: dormName };
            onSave(dormId, updatedDorm);
        } else {
            alert("Dorm name cannot be empty");
        }
    };

    if (!showModal) return null;

    return (
        <div className="modal-overlay" style={modalOverlayStyles}>
            <div className="modal-content" style={modalContentStyles}>
                <h2>Edit Dorm</h2>
                {loading ? (
                    <p>Loading...</p>
                ) : (
                    <>
                        <input
                            type="text"
                            value={dormName}
                            onChange={(e) => setDormName(e.target.value)}
                            placeholder="Dorm Name"
                            style={inputStyles}
                        />
                        <div style={buttonContainerStyles}>
                            <button onClick={handleSave} style={buttonStyles}>Save</button>
                            <button onClick={onClose} style={buttonStyles}>Cancel</button>
                        </div>
                    </>
                )}
            </div>
        </div>
    );

};

const modalOverlayStyles = {
    position: 'fixed',
    top: 0,
    left: 0,
    width: '100%',
    height: '100%',
    backgroundColor: 'rgba(0, 0, 0, 0.5)',
    display: 'flex',
    justifyContent: 'center',
    alignItems: 'center',
    zIndex: 1000,
};

const modalContentStyles = {
    background: '#fff',
    padding: '20px',
    borderRadius: '8px',
    width: '300px',
    textAlign: 'center',
};

const inputStyles = {
    width: '100%',
    padding: '10px',
    marginBottom: '15px',
    borderRadius: '5px',
    border: '1px solid #ccc',
};

const buttonContainerStyles = {
    display: 'flex',
    justifyContent: 'space-between',
};

const buttonStyles = {
    padding: '10px 20px',
    borderRadius: '5px',
    border: 'none',
    cursor: 'pointer',
    backgroundColor: '#007BFF',
    color: '#fff',
};
ModalDormUpdate.propTypes = {
    showModal: PropTypes.bool.isRequired,
    onClose: PropTypes.func.isRequired,
    dormId: PropTypes.number.isRequired,
    onSave: PropTypes.func.isRequired,
};

export default ModalDormUpdate;
