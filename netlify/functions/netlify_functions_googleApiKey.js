exports.handler = async function(event, context) {
  return {
    statusCode: 200,
    headers: {
      'Access-Control-Allow-Origin': '*', // restrict in production
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ apiKey: process.env.GOOGLE_API_KEY })
  };
};