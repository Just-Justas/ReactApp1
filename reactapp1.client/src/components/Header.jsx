import React from 'react';
import { useNavigate } from 'react-router-dom';

const Header = () => {
    const navigate = useNavigate();

    const handleNavigation = (route) => {
        navigate(route);
    };

    return (
        <header className="p-3 bg-primary text-white">
            <div className="container">
                <h1 className="mb-4">Universiteto bendrabučių forumas</h1>
                <table className="table table-borderless">
                    <tbody>
                        <tr>
                            <td>
                                <button
                                    onClick={() => handleNavigation('/')}
                                    className="btn btn-link text-white"
                                >
                                    Pradžia
                                </button>
                            </td>
                            <td>
                                <button
                                    onClick={() => handleNavigation('/dorms')}
                                    className="btn btn-link text-white"
                                >
                                    Bendrabučiai
                                </button>
                            </td>
                            <td>
                                <button
                                    onClick={() => handleNavigation('/login')}
                                    className="btn btn-link text-white"
                                >
                                    Prisijungti
                                </button>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </header>
    );
};

export default Header;
