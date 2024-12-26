import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import Home from './pages/Home';
import Dorms from './pages/Dorms';
import DormsCreate from './pages/DormsCreate';
import ResidentsCreate from './pages/ResidentsCreate';
import Login from './pages/Login';
import ResidentManagementPage from './components/ResidentManagementPage';
import PostsManagementPage from './components/PostsManagementPage'; // Import Posts Management Component
import './styles/ResponsiveLayout.css';
import Header from './components/Header';
import Footer from './components/Footer';

function App() {
    return (
        <Router>
            <Header />
            <main className="py-4">
                <Routes>
                    <Route path="/" element={<Home />} />
                    <Route path="/dorms" element={<Dorms />} />
                    <Route path="/dormsCreate" element={<DormsCreate />} />
                    <Route path="/login" element={<Login />} />
                    <Route path="/ResidentsCreate" element={<ResidentsCreate />} />

                    <Route path="/dorms/:dormId/residents" element={<ResidentManagementPage />} />

                    <Route path="/dorms/:dormId/posts" element={<PostsManagementPage />} />
                </Routes>
            </main>
            <Footer />
        </Router>
    );
}

export default App;
