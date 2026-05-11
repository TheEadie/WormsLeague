import type { AuthProviderProps } from 'react-oidc-context'
import { WebStorageStateStore } from 'oidc-client-ts'

const authority = 'https://eadie.eu.auth0.com/'
const clientId = 'nC2Zk62e2lIdgSoZPCThInvVGbltrPJ8'
const audience = 'worms.davideadie.dev'

export const oidcConfig: AuthProviderProps = {
    authority,
    client_id: clientId,
    redirect_uri: `${window.location.origin}/callback`,
    post_logout_redirect_uri: window.location.origin,
    scope: 'openid profile',
    extraQueryParams: { audience },
    automaticSilentRenew: true,
    userStore: new WebStorageStateStore({ store: window.localStorage }),
    onSigninCallback: () => {
        window.history.replaceState({}, document.title, window.location.pathname)
    },
}
