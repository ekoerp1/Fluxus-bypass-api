// next.config.js
module.exports = {
    assetPrefix: process.env.NODE_ENV === 'production' ? '/my-nextjs-app/' : '',
    basePath: process.env.NODE_ENV === 'production' ? '/my-nextjs-app' : '',
  };
  