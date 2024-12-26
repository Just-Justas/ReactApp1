import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import axios from 'axios';

const PostsManagementPage = () => {
    const { dormId } = useParams();
    const [posts, setPosts] = useState([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        fetchPosts();
    }, [dormId]);

    const fetchPosts = async () => {
        try {
            const response = await axios.get(`/api/dorms/${dormId}/posts`);
            setPosts(response.data);
            setLoading(false);
        } catch (error) {
            console.error('Error fetching posts:', error);
            setLoading(false);
        }
    };

    const handleDelete = async (postId) => {
        try {
            const token = localStorage.getItem('accessToken');
            await axios.delete(`/api/dorms/${dormId}/posts/${postId}`, {
                headers: { Authorization: `Bearer ${token}` },
            });
            setPosts(posts.filter((post) => post.id !== postId));
        } catch (error) {
            console.error('Error deleting post:', error);
        }
    };

    if (loading) return <p>Loading posts...</p>;

    return (
        <div className="container">
            <h2>Bendrabutis nr. {dormId}</h2>
            <table className="table table-striped">
                <thead>
                    <tr>
                        <th>Pavadinimas</th>
                        <th>Tekstas</th>
                        <th>Data ir laikas</th>
                        <th>Veiksmai</th>
                    </tr>
                </thead>
                <tbody>
                    {posts.length > 0 ? (
                        posts.map((post) => (
                            <tr key={post.id}>
                                <td>{post.title}</td>
                                <td>{post.text}</td>
                                <td>{new Date(post.posted_timestamp).toLocaleString()}</td>
                                <td>
                                    <button
                                        onClick={() => handleDelete(post.id)}
                                        className="btn btn-danger btn-sm"
                                    >
                                        Trinti
                                    </button>
                                </td>
                            </tr>
                        ))
                    ) : (
                        <tr>
                            <td colSpan="4" className="text-center">
                                Nėra įrašų
                            </td>
                        </tr>
                    )}
                </tbody>
            </table>
        </div>
    );
};

export default PostsManagementPage;
